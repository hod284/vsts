using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using VSTS;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace WebSpace
{
    public enum LOCALStatusCode
    {   //인터넷 망이 안되었을 경우
        CheckInternet =999,
        // 코드에서 나오는 오류
        Checkcode= 1000,
        // 클라이언트에서 서버에 연결할때 연결이 안되는 경우
        httpconncetionerror = 1001,
        // 서버가 죽었을경우
        KeepAliveFailure = 1002,
        // 서버응답하는 시간이 지났을경우
        Timeout = 1003,
        //서버에서 응답을 받을때 오류들
        ReceiveFailure = 1004,
        // 서버에서 보내온 오류들
        HttpStatusCode = 1005,
        // 비디오 리스트없을때
        VideoLIstisNull = 1006,
        // 액세스 토큰이 없을때
        AcessTokenisNULL = 1007,
        //url커리가 잘못되었을 경우 
        WrongQurery = 1008,
        // 받은데이터와 받아올 클래스 형식이 안맞을때
        AcessDataFormisWrong = 1009,
        // 리프레쉬토큰 없을때
        RefreshTokenisNULL = 1010
    }
    [System.Serializable]
    public class FileForm
    {
        public string Name = null;
        public string ContentType = null;
        public string FilePath = null;
        public Stream Stream;
    }
    public class RsponseClass<T>
    {
        public T data;
        public string statuscode_string;
        public int statuscode_int;
    }

    public class WebManager : MonoBehaviour
    {
        
        [System.Serializable]
        class ReturnCase
        {
            public bool success;
            public string error =null;
            public int statusCode;
            public string message = null;
            public string accessToken = null;
        }
        // 제이슨 리턴 포맷
        //{ success: "true/false", error: str, statusCode: xxx, message: str, data: { } }
        [SerializeField] private string Url;
        [SerializeField] private string AcessToken = null;
        [SerializeField] private string RefreshToken = null;
        [SerializeField] private WebConnnection WBConnnection;
        private Dictionary<string, object> VideoList = new Dictionary<string, object>();
        private string result = string.Empty;
        /// <summary>
        /// 엑세스 토큰
        /// </summary>
        public string GetAcessToken { get => AcessToken; }
        /// <summary>
        ///  리프레쉬 토큰
        /// </summary>
        public string GetRefreshToken { get => RefreshToken; }
        /// <summary>
        /// 웹주소 
        /// </summary>
        public string GetUrl { get => Url; }
        /// <summary>
        /// 웹주소  설정
        /// </summary>
        public string SetUrl(string url) => Url = url;
        /// <summary>
        /// 보내는 동영상 추가
        /// </summary>>
        public void VideoList_Add(string key, FileForm fileForm) => VideoList.Add(key, fileForm);
        /// <summary>
        /// 로그인
        /// </summary>
        public async UniTask<RsponseClass<T>> Login_Trainee(LoginRequest loginRequest)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/login";
            string Tojson = JsonConvert.SerializeObject(loginRequest);
            value = await TryConncetion<LoginAnswer>(seturl, Tojson,"POST", null);
            WBConnnection.GetResponeHeader_Public.TryGetValue("access_token", out AcessToken);
            WBConnnection.GetResponeHeader_Public.TryGetValue("Set-Cookie", out RefreshToken);
            if (AcessToken != null)
            {
                var resolve = Getinformation_token();
                SetInformation(resolve, ref value.data);
            }
            return value;
        }
        public async UniTask<RsponseClass<T>> Login_Instructor(LoginRequest loginRequest)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/loginInstructor";
            string Tojson = JsonConvert.SerializeObject(loginRequest);
            value = await TryConncetion<LoginAnswer>(seturl, Tojson, "POST", null);
            WBConnnection.GetResponeHeader_Public.TryGetValue("access_token", out AcessToken);
            WBConnnection.GetResponeHeader_Public.TryGetValue("Set-Cookie", out RefreshToken);
            if (AcessToken != null)
            {
                var resolve = Getinformation_token();
                SetInformation(resolve, ref value.data);
            }
            return value;
        }
        /// <summary>
        /// 버전 체크
        /// </summary>
        public async UniTask<RsponseClass<T>> GetVersion<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/version";
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 회원가입 여부 체크
        /// </summary>
        public async UniTask<RsponseClass<T>> GetCheckSignup(DuplicationRequest duplicationRequest)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string Tojson = JsonConvert.SerializeObject(duplicationRequest);
            string seturl = Url + "/auth/signup/check";
            value = await TryConncetion<DuplicationAnswer>(seturl, Tojson, "POST", new string[] { AcessToken, RefreshToken });
            return value;
        }
        /// <summary>
        /// 회원가입 
        /// </summary>
        public async UniTask<RsponseClass<T>> Signup_Trainee(RegistInLoginRequest Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/signup/trainee";
            string Tojson = JsonConvert.SerializeObject(Information);
            value = await TryConncetion<RegistInLoginAnswer>(seturl, Tojson, "POST", null);
            return value;
        }

        public async UniTask<RsponseClass<T>> Signup_Instructor(RegistInLoginRequest Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/signup/instructor";
            string Tojson = JsonConvert.SerializeObject(Information);
            value = await TryConncetion<T>(seturl, Tojson, "POST", null);
            return value;
        }

        public async UniTask<RsponseClass<T>> Signup_Admin(RegistAdminRequest Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/signup/manager";
            string Tojson = JsonConvert.SerializeObject(Information);
            value = await TryConncetion<T>(seturl, Tojson, "POST", null);
            return value;
        }
        /// <summary>
        /// 비밀번호 변경 
        /// </summary>
        public async UniTask<RsponseClass<T>> ChangePassword<T>(object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/auth/password/reset";
            string Tojson = JsonConvert.SerializeObject(Information);
            value = await TryConncetion<T>(seturl, Tojson, "PUT", null);
            return value;
        }
        /// <summary>
        /// 이론평가 문제 요청 
        /// </summary>
        public async UniTask<RsponseClass<T>> GetTheoryTest<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/test/theory";
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null,  "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 훈련시작 전송
        /// </summary>
        public async UniTask<RsponseClass<T>> SendingStartTrainning<T>(object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/begin";
            string Tojson = JsonConvert.SerializeObject(Information);
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, Tojson, "POST", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 훈련 상태 변경 전송
        /// </summary>
        public async UniTask<RsponseClass<T>> SendingTrainningStatus<T>( object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/status";
            string Tojson = JsonConvert.SerializeObject(Information);
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, Tojson, "PUT", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 이론 평가 문제 풀이결과 점수 전송
        /// </summary>
        public async UniTask<RsponseClass<T>> SendingTheoryTestPoint<T>(object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/test/theory";
            string Tojson = JsonConvert.SerializeObject(Information);
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, Tojson, "POST", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 실기 평가 문제 풀이결과 점수 전송
        /// </summary>
        public async UniTask<RsponseClass<T>> SendingPraticeTestPoint<T>( object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/test/practical";
            string Tojson = JsonConvert.SerializeObject(Information);
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, Tojson, "POST", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 특정 훈련생 결과 확인
        /// </summary>
        public async UniTask<RsponseClass<T>> GetOneTrainerResultCheck<T>( string ArmyNumber)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/trainee/"+ArmyNumber;
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 시나리오 받기
        /// </summary>
        public async UniTask<RsponseClass<T>> GetSenario<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/train/scenario/list";
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 부대 리스트 받기
        /// </summary>
        public async UniTask<RsponseClass<T>> GetUnits<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/api/units";
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 회원가입(admin)
        /// </summary>
        public async UniTask<RsponseClass<T>> Signup_admin<T>(object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/admin/signup";
            string Tojson = JsonConvert.SerializeObject(Information);
            value = await TryConncetion<T>(seturl, Tojson, "POST", null);
            return value;
        }
        /// <summary>
        /// 유저 리스트(admin)
        /// </summary>
        public async UniTask<RsponseClass<T>> GetUserList_admin<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/admin/user/list";
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "GET", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 특정 유저 정보 변경(admin)
        /// </summary>
        public async UniTask<RsponseClass<T>> UserChangeInformation_admin<T>( string ArmyNumber,object Information)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/admin/user/"+ ArmyNumber;
            string Tojson = JsonConvert.SerializeObject(Information);
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, Tojson, "PUT", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 특정 유저 정보 삭제(admin)
        /// </summary>
        public async UniTask<RsponseClass<T>> DeletUserInformation_admin<T>(string ArmyNumber)
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url + "/admin/user/" + ArmyNumber;
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "DELETE", new string[] { AcessToken, RefreshToken });
            }
            return value;
        }
        /// <summary>
        /// 파일 업로드
        /// </summary>
        public async UniTask<RsponseClass<T>> FileUpload<T>()
        {
            RsponseClass<T> value = new RsponseClass<T>();
            string seturl = Url;
            string error = null;
            if (CheckToken(ref error) != 0)
            {
                value.statuscode_int = CheckToken(ref error);
                value.data = default;
                value.statuscode_string = error;
            }
            if (VideoList == null || VideoList.Count == 0)
            {
                value.statuscode_int = 1006;
                value.statuscode_string = "localerror 보내는 동영상을 담는 Dictionary에 데이터 없음";
                Debug.LogError("1006 : localerror 보내는 동영상을 담는 Dictionary에 데이터 없음");
                value.data = default;
            }
            else
            {
                value = await TryConncetion<T>(seturl, null, "", new string[] { AcessToken, RefreshToken },1);
            }
            return value;
        }
        /// <summary>
        ///  웹통신 하기위해 통신 함수 부르기
        /// </summary>
        private async UniTask<RsponseClass<T>> TryConncetion <T>(string seturl, string Tojson, string method, string[] Token, int  methodselect =0)
        {
            RsponseClass<T> value = new RsponseClass<T>();

            await UniTask.RunOnThreadPool(async () => {
                if (methodselect == 0)
                    result = await WBConnnection.SendingInformation(seturl, method, Tojson, Token);
                else
                    result = await WBConnnection.SendingInformation_fileupload(seturl, VideoList, new string[] { AcessToken, RefreshToken });
                Debug.Log("밭은 데이터 json : " + result);
                try
                {
                    var caseresult = JsonUtility.FromJson<ReturnCase>(result);
                    SetAcessToken(caseresult.accessToken);
                    if (caseresult.success)
                    {
                        JObject jobj = JObject.Parse(result);
                        var data = jobj["data"];
                        value.data = JsonConvert.DeserializeObject<T>(data.ToString());
                    }
                    else
                    {
                        value.data = default(T);
                    }
                }
                catch (Exception e)
                {
                    value.data = default(T);
                    int Parse = 0;
                    int.TryParse(result, out Parse);
                    value.statuscode_int = Parse;
                    value.statuscode_string = WBConnnection.GetHTTPStatusCode;
                }
            });
            return value;
        }
        /// <summary>
        /// URL 커리 검사기
        /// </summary>
        private bool UrlQureryCheck(string CheckUrl, ref string error,ref int StatusCode)
        {
            bool check = false;
            if (CheckUrl[0] =='?')
                CheckUrl = CheckUrl.Replace("?", "");
            if (CheckUrl.Contains('&'))
            {
                var array = CheckUrl.Split('&');
                for (int i = 0; i < array.Length; i++)
                {
                    check = Splite(array[i]);
                    if (check == false)
                    {
                        StatusCode = 1008;
                        error = "쿼리 형식 틀림";
                        Debug.LogError("1008 : 쿼리 형식 틀림");
                        break;
                    }
                }
            }
            else
                check = Splite(CheckUrl);
            return check;
        }
        private bool Splite (string  splitetext)
        {
            var sp = splitetext.Split('=');
            if (sp[0] != string.Empty && sp[1] != string.Empty)
                return true;
            else
                return false;
        }
        /// <summary>
        /// AcessToken 갱신
        /// </summary>
        private void SetAcessToken( string token)
        {
            if (token != null)
                AcessToken = token;
        }
        /// <summary>
        /// 토큰 확인
        /// </summary>
        private int CheckToken(ref string error)
        {
            int n = 0;
            if (AcessToken == null)
            {
                n = 1007;
                error = "localerror AcessToken 없음";
                Debug.LogError("1007 : localerror AcessToken 없음");
            }
            else if (RefreshToken == null)
            {
                n = 1010;
                error = "localerror RefreshToken 없음";
                Debug.LogError("1011 : localerror RefreshToken 없음");
            }
            return n;
        }
        /// <summary>
        /// 형식 검사기
        /// </summary>
        private bool CheckTheData(string getdata, string setdata, ref string error, ref int StatusCode)
        {
            bool r = true;
            if (getdata.Contains("{"))
                getdata= getdata.Replace("{", "");
            if (getdata.Contains("}"))
                getdata = getdata.Replace("}", "");
            if (setdata.Contains("{"))
                setdata= setdata.Replace("{", "");
            if (setdata.Contains("}"))
                setdata=setdata.Replace("}", "");
            getdata = getdata.Replace("\n", "");
            setdata = setdata.Replace("\n", "");
            var ArrayGetdata = getdata.Split(',');
            var ArraySetdata = setdata.Split(',');
                for (int i = 0; i < ArraySetdata.Length; i++)
                {
                    var head = ArraySetdata[i].Split(":");
                    if (!getdata.Contains(head[0]))
                    {
                        r = false;
                        StatusCode = 1009;
                        error = $"localerror 서버에서 보내온 데이터에 {head[0]}이 들어가 있지 않습니다\n서버에서 보내온 데이터 {getdata}\n서버에서 보내온 데이터를 받을 형식 {setdata}";
                        Debug.LogError($"1009 : localerror 서버에서 보내온 데이터에 {head[0]}이 들어가 있지 않습니다\n서버에서 보내온 데이터 {getdata}\n서버에서 보내온 데이터를 받을 형식 {setdata}");
                        break;
                    }
                }
            return r;
        }
        public LoginAnswer Getinformation_token()
        {
           LoginAnswer value = new LoginAnswer(); 
            var tokenHandler = new JwtSecurityTokenHandler();
            if (AcessToken != null)
            {
                var token = tokenHandler.ReadJwtToken(AcessToken);
                string[] s = new string[10];
                int n = 0;
                foreach (var va in token.Payload)
                {
                    s[n] = va.Value.ToString();
                    n++;
                }
                value.playerID = s[9];     // playerID
                value.accountID = s[1];    // UID
                value.armyCode = s[4];
                value.name = s[0];
                value.rankCode = s[5];
                value.submarineCode = s[6];
                value.departmentCode = s[7];
                value.permissionCode = s[8];
            }
            return value;
        }
        public void SetInformation(LoginAnswer resolvelogin, ref LoginAnswer login)
        {
            if (login.playerID == null)
                login.playerID = resolvelogin.playerID;
            if (login.accountID == null)
                login.accountID = resolvelogin.accountID;
            if (login.armyCode == null)
                login.armyCode = resolvelogin.armyCode;
            if (login.name == null)
                login.name = resolvelogin.name;
            if (login.rankCode == null)
                login.rankCode = resolvelogin.rankCode;
            if (login.submarineCode == null)
                login.submarineCode = resolvelogin.submarineCode;
            if (login.departmentCode == null)
                login.departmentCode = resolvelogin.departmentCode;
            if (login.permissionCode == null)
                login.permissionCode = resolvelogin.permissionCode;
        }
        private void Resultmethod( string re)
        {
            result = re;
        }
        public string ReJson(string toJson, string[] optionArray)
        {
           string optionedJson = null;
           JObject jarray = JObject.Parse(toJson);
           JObject  jobj = new JObject();
           for(int i =0; i< optionArray.Length; i++)
             jobj.Add(optionArray[i], jarray[optionArray[i]]);
           optionedJson = jobj.ToString(Formatting.None); 
           Debug.Log("제이슨 선택된 항목만 추가된 형태\n"+ optionedJson);
           return optionedJson;
        }
        private void CheckAccessToken(ref LoginAnswer data)
        {
              _WBConnnection.GetResponeHeader.TryGetValue("access_token", out _AcessToken);
              _WBConnnection.GetResponeHeader.TryGetValue("Set-Cookie", out _RefreshToken);
                // 중복 로그인 hash
               string _loginAttemptHash;
              _WBConnnection.GetResponeHeader.TryGetValue("loginAttemptHash", out _loginAttemptHash);

              if (_loginAttemptHash != null)
              {
                  data.loginAttempHash = _loginAttemptHash;
                   return;
              }

               if (_AcessToken != null)
               {
                  var resolve = Getinformation_token();
                 SetInformation(resolve, ref data);
              }
        }
    }
}
