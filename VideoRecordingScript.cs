using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Evereal.VideoCapture;
using System.IO;
using Cysharp.Threading.Tasks;
using System;
using RenderHeads.Media.AVProMovieCapture;
using VSTS;
using UnityEngine.Assertions.Must;


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
    public void SetPcCamera(Camera camera) => _PCCamera = camera;
    public void SetVRCamera(Camera camera) => _VRCamera = camera;
    public Camera GetPcCamera { get => _PCCamera; }
    public Camera GetVRCamera { get => _VRCamera; }
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
             default:
                _MovieCapture.CameraSelector.Camera = _PCCamera;
                _MovieCapture.UnityAudioCapture = _VRCamera.GetComponent<CaptureAudioFromAudioListener>();
                break;
        }
        _AudioCapture.OnComplete += AudioCaptureSavePath;
    }
    // test로직
   private void Update()
   {
       if (Input.GetKey(KeyCode.F10))
          RecordingStart("12");
       if (Input.GetKey(KeyCode.F11))
           RecordingStop();
   }
   //
    public void RecordingStart(string filename)
    {
        if (!_MovieCapture.IsCapturing())
        {
            _AudioCapture.StartCapture();
            _MovieCapture.StartCapture();
            _OutputVideoFileName = filename;
            UnityEngine.Debug.Log("녹화 시작!");
        }
    }
    public void RecordingStop()
    {
        _AudioCapture.StopCapture();
        UnityEngine.Debug.Log("녹화 종료!");
    }
    
    private async UniTask Merge()
    {
       await UniTask.RunOnThreadPool(async () =>
        {
            try
            {
                if (File.Exists(_OutputFilePath))
                {
                    UnityEngine.Debug.Log("이전에 있던 파일 삭제");
                    File.Delete(_OutputFilePath);
                }
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
                string arguments = $"-i \"{_VideoFilePath}\" -i \"{_AudioFilePath}\" -filter_complex \"[0:a]volume={originalAudioVolume}[a0];[1:a]volume={newAudioVolume}[a1];[a0][a1]amerge=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v libvpx -c:a libvorbis  \"{_OutputFilePath}\"";

                Process process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit(); // FFmpeg 작업 완료 대기
                process.Close();
                UnityEngine.Debug.Log("오디오 병합 완료!");
                _MergeOnComplete?.Invoke();
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
        _MovieCapture.StopCapture(true,true);
        _AudioFilePath = args.SavePath;
        _VideoFilePath = _MovieCapture.LastFilePath;
        _OutputFilePath = _MovieCapture.OutputFolderPath + $"/{ _OutputVideoFileName}.webm";
        Merge().Forget();
    }
}


