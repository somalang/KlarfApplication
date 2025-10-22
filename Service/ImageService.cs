using System;
using System.IO;
using System.Windows.Media.Imaging; // WPF 기본 라이브러리

namespace KlarfApplication.Service
{
    public class ImageService
    {
        /// <summary>
        /// 멀티페이지 TIF 파일에서 특정 프레임(페이지)을 로드합니다.
        /// </summary>
        /// <param name="tifFilePath">TIF 파일 경로</param>
        /// <param name="frameNumber">로드할 프레임 번호 (1-based index, 즉 1부터 시작)</param>
        /// <returns>WPF에서 표시 가능한 BitmapSource</returns>
        public BitmapSource LoadTifFrame(string tifFilePath, int frameNumber)
        {
            // [csharp] 파일이 없거나 유효하지 않은 프레임 번호(1 미만)인 경우 null 반환
            if (string.IsNullOrEmpty(tifFilePath) || !File.Exists(tifFilePath) || frameNumber <= 0)
            {
                return null;
            }

            try
            {
                // 1. TIF 파일을 스트림으로 엽니다.
                using (Stream tifStream = new FileStream(tifFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // 2. TiffBitmapDecoder를 사용하여 TIF 파일을 로드합니다.
                    //    BitmapCacheOption.OnLoad는 파일을 즉시 로드하고 스트림을 닫을 수 있게 합니다.
                    TiffBitmapDecoder decoder = new TiffBitmapDecoder(tifStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                    // 3. 프레임 인덱스 (0-based) 계산
                    //    사용자는 "127번째" (1-based)를 요청하지만, TiffBitmapDecoder.Frames는 (0-based)입니다.
                    int frameIndex = frameNumber - 1;

                    // 4. 요청된 프레임이 TIF 파일의 전체 프레임 수 내에 있는지 확인
                    if (frameIndex >= 0 && frameIndex < decoder.Frames.Count)
                    {
                        // 5. 해당 프레임을 BitmapSource로 반환
                        return decoder.Frames[frameIndex];
                    }
                    else
                    {
                        // 요청된 프레임 번호가 유효 범위를 벗어남
                        System.Diagnostics.Debug.WriteLine($"Error: Frame {frameNumber} (index {frameIndex}) is out of range. Total frames: {decoder.Frames.Count}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // TIF 파일이 아니거나 손상된 경우
                System.Diagnostics.Debug.WriteLine($"TIF Loading Error: {ex.Message}");
                return null;
            }
        }
    }
}