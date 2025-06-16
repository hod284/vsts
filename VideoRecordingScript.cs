using Cysharp.Threading.Tasks;
using Evereal.VideoCapture;
using RenderHeads.Media.AVProMovieCapture;
using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using VSTS;



public class VideoRecordingScript : MonoBehaviour
{
    [SerializeField] private CaptureFromCamera _MovieCapture;
    [SerializeField] private Camera _PCCamera;
    [SerializeField] private Camera _VRCamera;
    [SerializeField] private AudioCapture _AudioCapture;
    [SerializeField] private string _VideoFilePath; // 합칠 동영상 파일 경로
    [SerializeField] private string _AudioFilePath; // 합칠 음성 파일 경로
    private string _OutputVideoFileName;
    [SerializeField] private string _OutputFilePath; // 결과 파일 경로
    public Action _MergeOnComplete;
    public Action _MergeOnError;
    private string _FolderName = "Recodings";
    private string _FFmpegPath = string.Empty;
    private string _Arguments = string.Empty;
    public void SetPcCamera(Camera camera) => _PCCamera = camera;
    public void SetVRCamera(Camera camera) => _VRCamera = camera;
    public Camera GetPcCamera { get => _PCCamera; }
    public Camera GetVRCamera { get => _VRCamera; }
    [Range(0f, 1f)]
    [SerializeField] private float originalAudioVolume = 1.0f; // 기존 오디오 음량 (0 ~ 1)
    [Range(0f, 1f)]
    [SerializeField] private float newAudioVolume = 0.0f; // 새로운 오디오 음량 (0 ~ 1)
    public void SetVideoFileName(string filename) => _OutputVideoFileName = filename;
    public string GetVideoFileName { get => _OutputVideoFileName; }
    public string GetVideoFolderPath { get => _MovieCapture.OutputFolderPath; }
    public CaptureFromCamera GetMovieCapture { get => _MovieCapture; }
    public string GetLocalVideoPath => _OutputFilePath;
    private System.Diagnostics.Process _Process = new System.Diagnostics.Process();

    // Start is called before the first frame update
    private void Awake()
    {
        _MergeOnComplete += FileDelete;
        _AudioCapture.OnComplete += AudioCaptureSavePath;
        _MovieCapture.OutputFolderPath = Path.Combine(Application.streamingAssetsPath, _FolderName);
        _AudioCapture.saveFolder = Path.Combine(Application.streamingAssetsPath, _FolderName);
        _FFmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe";
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            _FFmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe";
        }
        else if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            _FFmpegPath = Application.streamingAssetsPath + "/FFmpeg/x86/ffmpeg.exe";
        }
    }

    public void InitDevice()
    {

        if (_PCCamera != null)
        {
            if (!_PCCamera.GetComponent<CaptureAudioFromAudioListener>())
                _PCCamera.gameObject.AddComponent<CaptureAudioFromAudioListener>();
        }
        else
        {
            _PCCamera = Camera.main;
            if (!_PCCamera.GetComponent<CaptureAudioFromAudioListener>())
                _PCCamera.gameObject.AddComponent<CaptureAudioFromAudioListener>();
        }
        if (_VRCamera != null)
        {
            if (!_VRCamera.GetComponent<CaptureAudioFromAudioListener>())
                _VRCamera.gameObject.AddComponent<CaptureAudioFromAudioListener>();
        }
        else
        {
            _VRCamera = Camera.main;
            if (!_VRCamera.GetComponent<CaptureAudioFromAudioListener>())
                _VRCamera.gameObject.AddComponent<CaptureAudioFromAudioListener>();
        }
        Camera camera = null;
        switch (InputManager.Instance.InputType)
        {
            case E_INPUT_DEVICE.PC:
                camera = _PCCamera;
                break;
            case E_INPUT_DEVICE.VR:
                camera = _VRCamera;
                break;
        }
        _MovieCapture.CameraSelector.Camera = camera;
        _MovieCapture.UnityAudioCapture = camera.GetComponent<CaptureAudioFromAudioListener>();
    }

    public void RecordingStart()
    {
        if (!_MovieCapture.IsCapturing())
        {
            Debug.Log("StartVideoCap");
            _AudioCapture.StartCapture();
            _MovieCapture.StartCapture();
        }
    }

    public void RecordingStop()
    {
        if (_MovieCapture.IsCapturing())
        {
            if (_OutputVideoFileName == "")
                return;
            try
            {
                _AudioCapture.StopCapture();
            }
            catch( Exception e)
            {
                UnityEngine.Debug.Log("비디오만 변환");
                _MovieCapture.CompletedFileWritingAction += (FileWritingHandler) => ChangeVideo().Forget();
                 Debug.Log("fail_invoke");
                _MovieCapture.StopCapture();
            }
        }
    }
    private async UniTask ChangeVideo()
    {
        try
        {
            await UniTask.RunOnThreadPool(() =>
            {
                _VideoFilePath = _MovieCapture.LastFilePath;
                _OutputFilePath = _MovieCapture.OutputFolderPath + $"{_OutputVideoFileName}";
                string directoryPath = Path.GetDirectoryName(_OutputFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (File.Exists(_OutputFilePath))
                {
                    UnityEngine.Debug.Log("이전에 있던 파일 삭제");
                    File.Delete(_OutputFilePath);
                }
                _Arguments = $" -i {_VideoFilePath} {_OutputFilePath}";
               
                _Process.StartInfo.FileName = _FFmpegPath;
                _Process.StartInfo.Arguments = _Arguments;
                _Process.StartInfo.UseShellExecute = false;
                _Process.StartInfo.CreateNoWindow = true;
                _Process.Start();
                _Process.WaitForExit(); // FFmpeg 작업 완료 대기
                _Process.Close();
                _Process.Refresh();
                UnityEngine.Debug.Log("비디오 webm으로 변환 완료!");
                var list = _MovieCapture.CompletedFileWritingAction?.GetInvocationList();
                _MovieCapture.CompletedFileWritingAction -= (Action<FileWritingHandler>)list[list.Length - 1];
            });
            await UniTask.SwitchToMainThread();
            _MergeOnComplete?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError("Invalid Operation: " + ex.Message);
            _MergeOnError?.Invoke();
        }
        catch (ArgumentNullException ex)
        {
            Debug.LogError("Argument Null: " + ex.Message);
            _MergeOnError?.Invoke();
        }
        catch (ArgumentException ex)
        {
            Debug.LogError("Argument Exception: " + ex.Message);
            _MergeOnError?.Invoke();
        }
        catch (Win32Exception ex)
        {
            Debug.LogError("Win32 Exception: " + ex.Message);
            _MergeOnError?.Invoke();
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.LogError("Access Denied: " + ex.Message);
            _MergeOnError?.Invoke();
        }
        catch (IOException ex)
        {
            Debug.LogError($"IO Exception: {ex.Message} ,{ex.StackTrace}");
            _MergeOnError?.Invoke();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"비디오 변환 오류 발생: {e.Message} , {e.StackTrace}");
            _MergeOnError?.Invoke();
        }

    }
    

    private async UniTask Merge()
    {
       try
        {
             await UniTask.RunOnThreadPool(() =>
             {
            
                 string directoryPath = Path.GetDirectoryName(_OutputFilePath);
                 if (!Directory.Exists(directoryPath))
                 {
                     Directory.CreateDirectory(directoryPath);
                 }

/*                 if (File.Exists(_OutputFilePath))
                 {
                     UnityEngine.Debug.Log("이전에 있던 파일 삭제");
                     File.Delete(_OutputFilePath);
                 }*/

               

              
                 _Arguments = $"-i \"{_VideoFilePath}\" -i \"{_AudioFilePath}\" -filter_complex \"[0:a]volume={originalAudioVolume}[a0];[1:a]volume={newAudioVolume}[a1];[a0][a1]amix=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v libx264 -c:a aac -strict experimental \"{_OutputFilePath}\"";

                
                 _Process.StartInfo.FileName = _FFmpegPath;
                 _Process.StartInfo.Arguments = _Arguments;
                 _Process.StartInfo.UseShellExecute = false;
                 _Process.StartInfo.CreateNoWindow = true;
                 _Process.Start();
                 _Process.WaitForExit();
                 _Process.Close();
                 _Process.Refresh();
                 UnityEngine.Debug.Log("오디오 병합 완료!");
                 });
               await UniTask.SwitchToMainThread();
               _MergeOnComplete?.Invoke();
            }
             catch (InvalidOperationException ex)
             {
                 Debug.LogError("Invalid Operation: " + ex.Message);
                 _MergeOnError?.Invoke();
             }
             catch (ArgumentNullException ex)
             {
                 Debug.LogError("Argument Null: " + ex.Message);
                 _MergeOnError?.Invoke();
             }
             catch (ArgumentException ex)
             {
                 Debug.LogError("Argument Exception: " + ex.Message);
                 _MergeOnError?.Invoke();
             }
             catch (Win32Exception ex)
             {
                 Debug.LogError("Win32 Exception: " + ex.Message);
                 _MergeOnError?.Invoke();
             }
             catch (UnauthorizedAccessException ex)
             {
                 Debug.LogError("Access Denied: " + ex.Message);
                 _MergeOnError?.Invoke();
             }
             catch (IOException ex)
             {
                 Debug.LogError($"IO Exception: {ex.Message} {ex.StackTrace}");
                 _MergeOnError?.Invoke();
             }
             catch (System.Exception e)
             {
                 UnityEngine.Debug.LogError($"오디오 병합 중 오류 발생: {e.Message}, {e.StackTrace}");
                 _MergeOnError?.Invoke();
             }
    }
    private void AudioCaptureSavePath(object sender, CaptureCompleteEventArgs args)
    {
        _MovieCapture.StopCapture();
        _AudioFilePath = args.SavePath;
        _VideoFilePath = _MovieCapture.LastFilePath;
        _OutputFilePath = _MovieCapture.OutputFolderPath + $"{_OutputVideoFileName}";

        Merge().Forget();
    }

    private void FileDelete()
    {
        Debug.Log("파일 삭제 시도");

        if (File.Exists(_VideoFilePath))
            File.Delete(_VideoFilePath);
        

        
        if (File.Exists(_AudioFilePath))
            File.Delete(_AudioFilePath);
        
        
        
    }
}


