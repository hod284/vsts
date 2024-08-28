using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WebSpace
{
    public class WebConnnection : MonoBehaviour
    {
        private Dictionary<string, string> GetResponeHeader = new Dictionary<string, string>();
        /// <summary>
        /// 응답 HTTP Header dictionary
        /// </summary>
        public Dictionary<string,string> GetResponeHeader_Public { get => GetResponeHeader; }
        /// <summary>
        /// 응답 HTTP Header dictionary
        /// </summary>
        [SerializeField] private int TimeOutTime ;
        private string HTTPStatusCode;
        /// <summary>
        ///  httpstatuscode
        /// </summary>
        public string GetHTTPStatusCode { get => HTTPStatusCode; }
        public async UniTask<string> SendingInformation(string Url, string Method, string Jsonbody = null,string[] Token = null)
        {
            bool caninternet=false;
            // 메인쓰레드로 스위치
            await UniTask.SwitchToMainThread();
            caninternet= CheckNetwork();
            // 스레드 풀로 스위치
            await UniTask.SwitchToThreadPool();

            if (caninternet == false)
                return "999";
            string returnstr = string.Empty;
            byte[] sendData = null;
            try
            {
                HttpWebRequest httpWebRequest = null;
                if(Jsonbody != null)
                   sendData = UTF8Encoding.UTF8.GetBytes(Jsonbody);
                if (Method == "POST"|| Method == "PUT" || Method == "PATCH")
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = Method;
                    httpWebRequest.Timeout = TimeOutTime;
                    httpWebRequest.ContentLength = sendData.Length;
                    //post put patch 같이 데이터를 묶어서 보내는 경우 무조건 데이터를 스트림 만들어서 넣기 전에 헤더나 request에 필요한 요소들을 미리넣어야지 에러가 안난다
                    Sethead(Token, ref httpWebRequest);
                    using (Stream requestStream = httpWebRequest.GetRequestStream())
                    {
                        requestStream.Write(sendData, 0, sendData.Length);
                        requestStream.Flush();
                        requestStream.Close();
                    }
                }
                else if (Method == "GET"|| Method == "DELETE")
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
                    httpWebRequest.Timeout = TimeOutTime;
                    httpWebRequest.Method = Method;
                    Sethead(Token, ref httpWebRequest);
                }

                HttpWebResponse httpWebResponse;
                using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    GetResponeHeader.Clear();
                    for (int i = 0; i < httpWebResponse.Headers.Count; i++)
                        GetResponeHeader.Add(httpWebResponse.Headers.Keys[i], httpWebResponse.Headers[i]);
                    StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    string result = streamReader.ReadToEnd();
                    returnstr = result;
                }
            }
            catch (WebException e)
            {
                returnstr = DebuglogError(e);
            }
            catch (Exception e)
            {
                Debug.LogError("1000 : ExcepError " + e.Message);
                returnstr = "1000";
                HTTPStatusCode = "로직에러";
            }
            return returnstr;
        }
        /// <summary>
        /// 파일 업로드용 함수
        /// </summary>
        public async UniTask<string> SendingInformation_fileupload(string Url, Dictionary<string, object> Files, string[] Token = null)
        {
            bool caninternet = false;
            // 메인쓰레드로 스위치
            await UniTask.SwitchToMainThread();
            caninternet = CheckNetwork();
            // 스레드 풀로 스위치
            await UniTask.SwitchToThreadPool();

            if (caninternet == false)
                return "999";
            string returnstr = string.Empty;
            HttpWebRequest httpWebRequest = null;
            try
            {
                string boundary = "-----------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpWebRequest.Method = "POST";
                httpWebRequest.KeepAlive = true;
                httpWebRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
                httpWebRequest.Timeout = TimeOutTime;
                httpWebRequest.Headers.Add("Authorization", "Bearer " + Token[0]);
                httpWebRequest.Headers.Add("Cookie", Token[1]);
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    foreach (KeyValuePair<string,object> pair in Files)
                    {
                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        if (pair.Value is FileForm)
                        {
                            FileForm file = pair.Value as FileForm;
                            string header = "Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.Name + "\"\r\nContent-Type: " + file.ContentType + "\r\n\r\n";
                            byte[] bytes = Encoding.UTF8.GetBytes(header);
                            requestStream.Write(bytes, 0, bytes.Length);

                            byte[] buffer = new byte[32768];
                            int bytesRead;
                            // upload from file
                            if (file.Stream == null)
                            {
                                using (FileStream fileStream = File.OpenRead(file.FilePath))
                                {
                                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        requestStream.Write(buffer, 0, bytesRead);
                                        fileStream.Close();
                                    }
                                }
                            }
                            else
                            {
                                while ((bytesRead = file.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                    requestStream.Write(buffer, 0, bytesRead);
                            }
                         
                        }
                        else
                        {
                            string data = "Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n\r\n" + pair.Value;
                            byte[] bytes = Encoding.UTF8.GetBytes(data);
                            requestStream.Write(bytes, 0, bytes.Length);
                        }
                        byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                        requestStream.Write(trailer, 0, trailer.Length);
                        requestStream.Close();
                        HttpWebResponse httpWebResponse;
                        using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                        {
                            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                            string result = streamReader.ReadToEnd();
                            returnstr = result;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                returnstr = DebuglogError(e);
            }
            catch (Exception e)
            {
                Debug.LogError("1000 : ExcepError " + e.Message);
                returnstr = "1000";
                HTTPStatusCode = "로직에러";
            }
            return returnstr;
        }
        /// <summary>
        /// 로컬 컴퓨터 인터넷 체크 함수
        /// </summary>
        private bool CheckNetwork()
        {
            bool net = true;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("999 : 네트워크 연결 안됨");
                net = false;
                HTTPStatusCode = "네트워크 연결 안됨";
            }
            return net;
        }
        /// <summary>
        /// 리퀘스트 헤드 저장하는 함수
        /// </summary> 
        private void Sethead(string[] Token, ref HttpWebRequest httpWebRequest)
        {
            if (Token != null)
            {
                httpWebRequest.Headers.Add("Authorization", "Bearer " + Token[0]);
                httpWebRequest.Headers.Add("Cookie", Token[1]);
            }
        }
        /// <summary>
        /// web exception 에러 모아놓은 함수
        /// </summary>
        private string DebuglogError(WebException oWEx)
        {
            WebExceptionStatus oStatus = oWEx.Status;
            string strTimeoutErrorMessage = "A connection attempt failed because the connected party did not properly respond "
                                          + "after a period of time, or established connection failed because connected host has failed to respond";
            string value = string.Empty;
            switch (oStatus)
            {
                case WebExceptionStatus.KeepAliveFailure:
                    if ((oWEx.InnerException != null) && (oWEx.InnerException.InnerException != null)
                        && oWEx.InnerException.InnerException.Message.ToString().Equals(strTimeoutErrorMessage, StringComparison.CurrentCultureIgnoreCase))
                    {   //----------------------------------------------------------------------
                        // This is Timeout Error which is wrongly thrown as a ReceiveFailure for
                        // SSL requests under this special condition.
                        //
                        // Handle this as a Timeout Error
                        //----------------------------------------------------------------------
                        Debug.LogError("1001 :  WebException error->This is Timeout Error which is wrongly thrown as a ReceiveFailure for SSL requests under this special condition as Timeout");
                        HTTPStatusCode = " WebException error-> This is Timeout Error which is wrongly thrown as a ReceiveFailure for  SSL requests under this special condition as Timeout.";
                        value = "1001";
                    }
                    else
                    {
                        Debug.LogError("1002 : WebException error  KeepAliveFailure");
                        value = "1002";
                        HTTPStatusCode = "WebException error  KeepAliveFailure";
                    }

                    break;
                case WebExceptionStatus.Timeout:
                    //----------------------------------------------------------------------
                    // This is a Timeout.
                    //----------------------------------------------------------------------
                    Debug.LogError("1003 :  WebException error  Timeout");
                    value = "1003";
                    HTTPStatusCode = "Timeout.";
                    break;
                case WebExceptionStatus.ReceiveFailure:
                    if ((oWEx.InnerException != null)
                        && (oWEx.InnerException.InnerException != null)
                        && oWEx.InnerException.InnerException.Message.ToString().Equals(strTimeoutErrorMessage, StringComparison.CurrentCultureIgnoreCase))
                    {    //----------------------------------------------------------------------
                         // This is Timeout Error which is wrongly thrown as a ReceiveFailure for
                         // SSL requests under this special condition.
                         //
                         // Handle this as a Timeout Error
                         //----------------------------------------------------------------------
                        Debug.LogError("1001 :  WebException error-> This is Timeout Error which is wrongly thrown as a ReceiveFailure for  SSL requests under this special condition as Timeout");
                        value = "1001";
                        HTTPStatusCode = " WebException error-> This is Timeout Error which is wrongly thrown as a ReceiveFailure for  SSL requests under this special condition as Timeout.";
                    }
                    else
                    {   
                        Debug.LogError("1004 :  WebException error ReceiveFailure.");
                        value = "1004";
                        HTTPStatusCode = "WebException error ReceiveFailure.";
                    }
                    break;
                default:
                    //----------------------------------------------------------------------
                    //  This is some other Exception
                    //----------------------------------------------------------------------
                    Debug.LogError("1005 :  WebException error" + oWEx.Message);
                    value = "1005";
                    HTTPStatusCode = oWEx.Message;
                    HTTPStatusCode =Regex.Replace(HTTPStatusCode, @"[^0-9]", "");
                    break;
            }
            return value;
        }
    }
}




