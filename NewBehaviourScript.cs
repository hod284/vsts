using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Evereal.VideoCapture;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Net;
using System;
using RenderHeads.Media.AVProMovieCapture;
using Mono.Cecil.Cil;
using BNG;
using VSTS;


public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private CaptureFromCamera _MovieCapture;
    [SerializeField] private Camera _PCCamera;
    [SerializeField] private Camera _VRCamera;
    [SerializeField] private AudioCapture _AudioCapture;
    [SerializeField] private string _VideoFilePath; // 합칠 동영상 파일 경로
    [SerializeField] private string _AudioFilePath; // 합칠 음성 파일 경로
    private string _OutputVideoFileName;
    private string _OutputFilePath; // 결과 파일 경로
    public Action _UploadOnComplete;
    public Action _MergeOnComplete;
    public Action _MergeOnError;
    public event EventHandler<ResponseFTP> _UploadError;
    public event EventHandler<ResponseFTP> _FTPMakingFolderError;

    [Range(0f, 1f)]
    [SerializeField] private float originalAudioVolume = 1.0f; // 기존 오디오 음량 (0 ~ 1)
    [Range(0f, 1f)]
    [SerializeField] private float newAudioVolume = 0.0f; // 새로운 오디오 음량 (0 ~ 1)
    public void SetVideoFileName(string filename) => _OutputVideoFileName = filename;
    public string GetVideoFileName {  get=> _OutputVideoFileName; }
    // Start is called before the first frame update
    void Start()
    {
         switch (InputManager.Instance.InputType) 
        {
            case E_INPUT_DEVICE.PC:
            _MovieCapture.CameraSelector.Camera = _PCCamera;
            _MovieCapture.UnityAudioCapture = _PCCamera.GetComponent<CaptureAudioFromAudioListener>();
                break;
            case E_INPUT_DEVICE.VR:
                _MovieCapture.CameraSelector.Camera = _VRCamera;
            _MovieCapture.UnityAudioCapture = _VRCamera.GetComponent<CaptureAudioFromAudioListener>();
              break ;
        }
        _AudioCapture.OnComplete += AudioCaptureSavePath;
    }
    public async UniTask FtpUpload(string ftpPath, FileInfo file)
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
                req.Credentials = new NetworkCredential("anonymous", "");

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
                    int code = (int)response.StatusCode;
                    _UploadError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
                else if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    UnityEngine.Debug.LogFormat("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    UnityEngine.Debug.LogFormat("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                    int code = (int)((HttpWebResponse)e.Response).StatusCode;
                    _UploadError?.Invoke(this , new ResponseFTP (code, ((HttpWebResponse)e.Response).StatusDescription));
                }
                else
                {
                    UnityEngine.Debug.Log("Error: " + e.Message);
                    int code = (int)response.StatusCode;
                    _UploadError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("CodeError: " + e.Message);
            }
        });
    }
    public async UniTask MaketheFtpFIle(string ftpPath)
    {
        await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                FtpWebRequest requestFTPUploader = (FtpWebRequest)WebRequest.Create(ftpPath);
                
                requestFTPUploader.Credentials = new NetworkCredential("anonymous", "");
                
                requestFTPUploader.Method = WebRequestMethods.Ftp.MakeDirectory;
        
                using (FtpWebResponse resp = (FtpWebResponse)requestFTPUploader.GetResponse())
                {
                    resp.Close();
                }
            }
            catch (WebException e)
            {
                FtpWebResponse response = (FtpWebResponse)e.Response;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                   UnityEngine.Debug.LogFormat("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    UnityEngine.Debug.LogFormat("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                    int code = (int)((HttpWebResponse)e.Response).StatusCode;
                    _FTPMakingFolderError?.Invoke(this, new ResponseFTP(code, ((HttpWebResponse)e.Response).StatusDescription));
                }
                else
                {
                    UnityEngine.Debug.Log("Error: " + e.Message);
                    int code = (int)response.StatusCode;
                    _FTPMakingFolderError?.Invoke(this, new ResponseFTP(code, response.StatusDescription));
                }   
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("CodeError: " + e.Message);
            }
        });
    }
    public void RecordingStart(string filename)
    {
        if (!_MovieCapture.IsCapturing())
        {
            _MovieCapture.StartCapture();
            _AudioCapture.StartCapture();
            _OutputVideoFileName = filename;
            UnityEngine.Debug.Log("녹화 시작!");
        }
    }
    public void RecordingStop()
    {
        _AudioCapture.StopCapture();
        UnityEngine.Debug.Log("녹화 종료!");
    }
    public List<FileInfo> GetFileListInfos(string folderpath)
    {
        var files = new List<FileInfo>();
        var di = new DirectoryInfo(folderpath); 
        files = di.EnumerateFiles().ToList();
        return files;
    }

    private async UniTask Merge()
    {
       await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                // FFmpeg 실행 파일 경로 설정 (플랫폼에 따라 변경)
                string ffmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe"; // Windows 예시
                if (Application.platform == RuntimePlatform.OSXPlayer)
                {
                   ffmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe";
                }
                else if (Application.platform == RuntimePlatform.LinuxPlayer)
                {
                   ffmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe";
                }

                // FFmpeg 명령어 생성
                string arguments = $"-i \"{_VideoFilePath}\" -i \"{_AudioFilePath}\" -filter_complex \"[0:a]volume={originalAudioVolume}[a0];[1:a]volume={newAudioVolume}[a1];[a0][a1]amerge=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v copy -c:a aac -ac 2 \"{_OutputFilePath}\"";

                Process process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                process.WaitForExit(); // FFmpeg 작업 완료 대기
                UnityEngine.Debug.Log("오디오 병합 완료!");
                process.WaitForExit();
                UnityEngine.Debug.Log("동영상과 음성 합치기 완료!");
                DateTime dateTime = DateTime.Now;
                string repack = _MovieCapture.OutputFolderPath + $"/"+ _OutputVideoFileName+".webm";
                arguments = $"-i {_OutputFilePath}  -c:v libvpx -crf 30 -b:v 0   {repack}";
                process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                UnityEngine.Debug.Log("동영상체인지 완료");
                // 원본 파일 삭제
                if (File.Exists(_VideoFilePath))
                {
                    UnityEngine.Debug.Log("비디오 파일 삭제완료");
                    File.Delete(_VideoFilePath);
                }
                if (File.Exists(_AudioFilePath))
                {
                    UnityEngine.Debug.Log("오디오 파일 삭제완료");
                    File.Delete(_AudioFilePath);
                }
                if (File.Exists(repack))
                {
                    UnityEngine.Debug.Log("원본 동영상 삭제 완료");
                    File.Delete(_OutputFilePath);
                }
                _MergeOnComplete?.Invoke();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("오디오 병합 중 오류 발생: " + e.Message);
                _MergeOnError?.Invoke();
            }
        });
    }
 
    
    private void AudioCaptureSavePath(object sender, CaptureCompleteEventArgs args)
    {
        _MovieCapture.StopCapture();
        _AudioFilePath = args.SavePath;
        _VideoFilePath = _MovieCapture.LastFilePath;
        _OutputFilePath = _MovieCapture.OutputFolderPath + "/final.mp4";
        Merge().Forget();
    }
    public async UniTask FileUpload(string ftpPath,string FilePath,bool NewMakeFilePath =false)
    {
        string NewftpPath = ftpPath;
        if (NewMakeFilePath)
        {
            await MaketheFtpFIle(ftpPath);
            NewftpPath = ftpPath;
        }
        var fileinfor = new FileInfo(FilePath);
        if (fileinfor != null)
        {
            await FtpUpload(NewftpPath, fileinfor);
            if (File.Exists(fileinfor.FullName))
            {
                File.Delete(fileinfor.FullName);
                UnityEngine.Debug.Log("업로드 후 파일 삭제완료");
            }
        }
        else
            UnityEngine.Debug.Log("File isn't Found"); 
    }
    public async UniTask ListUpload(string ftpPath, List<FileInfo> files, bool NewMakeFilePath = false)
    {
        string NewftpPath = ftpPath;
        if (NewMakeFilePath)
        {
            await MaketheFtpFIle(ftpPath);
            NewftpPath = ftpPath;
        }
        if (files != null)
        {
            for (int i = 0; i < files.Count; i++)
            {
                await FtpUpload(NewftpPath, files[i]);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                    UnityEngine.Debug.Log("업로드 후 파일 삭제완료");
                }
            }
        }
        else
            UnityEngine.Debug.Log("List is null");
    }
    public void FileDelete(string filepath)
    {
        var file = new FileInfo(filepath);
        if (file.Exists)
        {
            UnityEngine.Debug.Log("file not found");
        }
        else
        {
            File.Delete(file.FullName);
            UnityEngine.Debug.Log("업로드 후 파일 삭제완료");
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

