using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Net;
using System;
using System.Text.RegularExpressions;

public class FTPUploadScript : MonoBehaviour
{
    public Action _UploadOnComplete;
    public event EventHandler<ResponseFTP> _UploadError;
    public event EventHandler<ResponseFTP> _FTPMakingFolderError;
    [SerializeField] private string _FtpAdress = "ftp://192.168.0.25";
    [SerializeField] private string _FolderPath = "/2021/12.webm";
    private string _UserID;
    private string _Password;
    public void SetFtpAdress(string Adress) => _FtpAdress = Adress;
    public string GetFtpAdress() => _FtpAdress;
    public void SetFolderPath(string PATH) => _FolderPath =PATH;
    public string GetFolderPath() => _FolderPath;
    //test
    //private void Start()
    //{
        //  FileUpload("hangil", "2486", "ftp://192.168.0.7/2024/07/03/", "C:\\Recordings/13_0_144_S5_J3_D1_150951_38.webm").Forget();
        //  FileUpload("anonymous", "", _FtpAdress+_FolderPath, "C:\\Recordings/12.webm").Forget();
    //}
    //
    private async UniTask FtpUpload(string ftpPath,string id ,string password ,FileInfo file)
    {
        await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                int uploadedBytes = 0;

                // WebRequest.Create로 Http,Ftp,File Request 객체를 모두 생성할 수 있다.
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpPath + file.Name);

                // FTP 업로드한다는 것을 표시
                req.Method = WebRequestMethods.Ftp.UploadFile;

                // 쓰기 권한이 있는 FTP 익명사용자 로그인 지정
                req.Credentials = new NetworkCredential(id, password);

                long totalBytes = file.Length;

                // RequestStream에 데이타를 쓴다
                using (Stream fileStream = File.OpenRead(file.ToString()))
                using (Stream reqStream = req.GetRequestStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        reqStream.Write(buffer, 0, buffer.Length);
                        uploadedBytes += bytesRead;
                        // Progress bar 업데이트
                        float progress = (float)uploadedBytes / totalBytes;
                        UnityEngine.Debug.Log($"{(int)(progress * 100)}%");
                    }
                }

                // FTP Upload 실행
                using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                {
                    // FTP 결과 상태 출력
                    UnityEngine.Debug.LogFormat("Upload: {0}{1}", file.Name, resp.StatusDescription);
                    resp.Close();
                    _UploadOnComplete?.Invoke();
                }
            }
            catch (WebException e)
            {
                FtpWebResponse response = (FtpWebResponse)e.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    UnityEngine.Debug.Log("Does not exist");
                    string scode = Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _UploadError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
                else if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    UnityEngine.Debug.LogFormat("Status Code : {0}", response.StatusCode);
                    UnityEngine.Debug.LogFormat("Status Description : {0}", response.StatusDescription);
                    string scode = Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _UploadError?.Invoke(this , new ResponseFTP (code, response.StatusDescription));
                }
                else
                {
                    string scode = Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _UploadError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("CodeError: " + e.Message);
            }
        });
    }
    public async UniTask MaketheFtpFIle(string id, string password, string ftpPath)
    {
        await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                FtpWebRequest requestFTPUploader = (FtpWebRequest)WebRequest.Create(ftpPath);
                
                requestFTPUploader.Credentials = new NetworkCredential(id, password);
                
                requestFTPUploader.Method = WebRequestMethods.Ftp.MakeDirectory;
                
               
                using (FtpWebResponse resp = (FtpWebResponse)requestFTPUploader.GetResponse())
                {
                    resp.Close();
                }
            }
            catch (WebException e)
            {
                FtpWebResponse response = (FtpWebResponse)e.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    UnityEngine.Debug.Log("Does not exist");
                    string scode = Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _UploadError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
                else if (e.Status == WebExceptionStatus.ProtocolError)
                {
                   UnityEngine.Debug.LogFormat("Status Code : {0}", response.StatusCode);
                    UnityEngine.Debug.LogFormat("Status Description : {0}", response.StatusDescription);
                    string scode =   Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _FTPMakingFolderError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
                else
                {
                    UnityEngine.Debug.LogFormat("Status Code : {0}", response.StatusCode);
                    UnityEngine.Debug.LogFormat("Status Description : {0}", response.StatusDescription);
                    string scode = Regex.Replace(response.StatusDescription, @"[^0-9]", "");
                    int code = 0;
                    int.TryParse(scode, out code);
                    _FTPMakingFolderError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }   
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("CodeError: " + e.Message);
            }
        });
    }  
    public List<FileInfo> GetFileListInfos(string folderpath)
    {
        var files = new List<FileInfo>();
        var di = new DirectoryInfo(folderpath); 
        files = di.EnumerateFiles().ToList();
        return files;
    }
    public async UniTask FileUpload(string id,string password, string ftpPath,string FilePath)
    {
        string NewftpPath = ftpPath;
        _UserID = id;
        _Password = password;
        if (!CheckDirectoryExists(id, password, ftpPath))
        { 
            await MaketheFtpFIle(id,password,ftpPath);
            await SetFolderPermissions(id, password, ftpPath);
            NewftpPath = ftpPath;
        }
        var fileinfor = new FileInfo(FilePath);
        if (fileinfor != null)
        {
            await FtpUpload(NewftpPath,id,password,fileinfor);
            if (File.Exists(fileinfor.FullName))
            {
                File.Delete(fileinfor.FullName);
                UnityEngine.Debug.Log("업로드 후 파일 삭제완료");
            }
        }
        else
            UnityEngine.Debug.Log("파일 없음"); 
    }
    public async UniTask ListUpload(string id, string password, string ftpPath, List<FileInfo> files)
    {
        string NewftpPath = ftpPath;
        _UserID = id;
        _Password = password;
        if (!CheckDirectoryExists(id, password, ftpPath))
        {
            await MaketheFtpFIle(id, password, ftpPath);
            await SetFolderPermissions(id, password, ftpPath);
            NewftpPath = ftpPath;
        }
        if (files != null)
        {
            for (int i = 0; i < files.Count; i++)
            {
                await FtpUpload(NewftpPath,id,password, files[i]);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                    UnityEngine.Debug.Log("업로드 후 파일 삭제완료");
                }
            }
        }
        else
            UnityEngine.Debug.Log("파일 리스트 없음");
    }
    public void FileDelete(string filepath)
    {
        var file = new FileInfo(filepath);
        if (!file.Exists)
        {
            UnityEngine.Debug.Log("파일 없음");
        }
        else
        {
            File.Delete(file.FullName);
            UnityEngine.Debug.Log("파일 삭제 완료");
        }
    }
    public bool StorageCheck()
    {
        bool CanUse =false;
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        DriveInfo ThisDrive = null;
        foreach (DriveInfo d in allDrives)
        {
            if (d.IsReady == true && d.Name == "C:\\")
                ThisDrive = d;
        }
        if (ThisDrive.AvailableFreeSpace == 0)
            CanUse = false;
        else
            CanUse = true;
        return CanUse;
    }
    public bool CheckDirectoryExists(string id ,string password, string ftppath)
    {
        try
        {
            // FTP 요청 생성 (ListDirectory 명령어 사용)
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftppath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            // 사용자 인증 정보 설정
            request.Credentials = new NetworkCredential(id, password);

            // 응답 받기
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                // 디렉토리가 존재하는 경우
                UnityEngine.Debug.Log("폴더 있음");
                return true;
            }
        }
        catch (WebException ex)
        {
            // 디렉토리가 없거나 오류가 발생한 경우
            if (ex.Response is FtpWebResponse response)
            {
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable || response.StatusCode == FtpStatusCode.NotLoggedIn)
                {
                    UnityEngine.Debug.Log("폴더 없음");
                }
            }
            return false;
        }
    }
    public async UniTask SetFolderPermissions(string id, string password, string ftpPath)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpPath);

            request.Method = "SITE CHMOD 755 " + _FolderPath;  // FTP 명령 전송
            request.Credentials = new NetworkCredential(id, password);
            request.UsePassive = true;
            request.KeepAlive = false;

            string chmodCommand = $"CHMOD {true} {_FolderPath}";

            // 명령 전송
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes(chmodCommand);
                requestStream.Write(commandBytes, 0, commandBytes.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                UnityEngine.Debug.Log($"폴더 권한 설정 완료: {response.StatusDescription}");
            }
        }
        catch (WebException e)
        {
            FtpWebResponse response = (FtpWebResponse)e.Response;
            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                UnityEngine.Debug.Log("폴더 권한 설정 중 오류 발생 \nDoes not exist");
            }
            else if (e.Status == WebExceptionStatus.ProtocolError)
            {
                UnityEngine.Debug.LogFormat("폴더 권한 설정 중 오류 발생 \nStatus Code : {0}", response.StatusCode);
                UnityEngine.Debug.LogFormat("폴더 권한 설정 중 오류 발생 \nStatus Description : {0}", response.StatusDescription);
            }
            else
            {
                UnityEngine.Debug.LogFormat("폴더 권한 설정 중 오류 발생 \nStatus Code : {0}", response.StatusCode);
                UnityEngine.Debug.LogFormat("폴더 권한 설정 중 오류 발생 \nStatus Description : {0}", response.StatusDescription);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"폴더 권한 설정 중 오류 발생: {ex.Message}");
        }
    }
    public List<string> GetFileList(string ftpAddress, string username, string password)
    {
        List<string> fileList = new List<string>();

        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    fileList.Add(line);
                }
                if (fileList.Count == 0)
                    UnityEngine.Debug.Log("파일 없음");
            }
        }
        catch (WebException e)
        {
            FtpWebResponse response = (FtpWebResponse)e.Response;
            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                UnityEngine.Debug.Log("Does not exist");
            }
            else if (e.Status == WebExceptionStatus.ProtocolError)
            {
                UnityEngine.Debug.LogFormat("Status Code : {0}", response.StatusCode);
                UnityEngine.Debug.LogFormat("Status Description : {0}", response.StatusDescription);
            }
            else
            {
                UnityEngine.Debug.LogFormat("Status Code : {0}", response.StatusCode);
                UnityEngine.Debug.LogFormat("Status Description : {0}", response.StatusDescription);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("FTP 오류: " + e.Message);
        }

        return fileList;
    }
    public string GetVideoURL(string ftpAddress, string ID, string password)
    {
        string url = string.Empty;
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
        request.Method = WebRequestMethods.Ftp.GetFileSize;
        request.Credentials = new NetworkCredential(ID, password);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            url = "ftp://" + ID + ":" + password+"@";
            ftpAddress = ftpAddress.Replace("ftp://", "");
            url += ftpAddress;
        }
        return url;
    }
}
public class ResponseFTP
{
    public int StateCode;
    public string StatusDescription;
    public ResponseFTP(int code , string description)
    { 
         StateCode = code;
        StatusDescription = description;
    }
}

