using System;
using System.IO;
using System.Windows; // ⭐️ [추가] Int32Rect
using System.Windows.Media; // ⭐️ [추가] PixelFormats
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

        // ⭐️ --- [추가 시작] ---

        /// <summary>
        /// BitmapSource에 밝기와 대비 효과를 적용합니다.
        /// (DefectImageView.xaml.cs의 ApplyEffects 로직 이동)
        /// </summary>
        /// <param name="originalImage">원본 이미지</param>
        /// <param name="brightness">밝기 값 (-100 ~ 100)</param>
        /// <param name="contrast">대비 값 (-100 ~ 100)</param>
        /// <returns>효과가 적용된 새 BitmapSource</returns>
        public BitmapSource ApplyBrightnessContrast(BitmapSource originalImage, double brightness, double contrast)
        {
            if (originalImage == null) return null;

            try
            {
                // 픽셀 접근이 용이하도록 BGRA32 형식으로 변환합니다.
                FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(originalImage, PixelFormats.Bgra32, null, 0);
                WriteableBitmap wBitmap = new WriteableBitmap(formattedBitmap);
                int width = wBitmap.PixelWidth; int height = wBitmap.PixelHeight; int stride = wBitmap.BackBufferStride;
                byte[] pixels = new byte[height * stride];
                wBitmap.CopyPixels(pixels, stride, 0);

                // 대비 계수 계산
                double contrastFactor = (100.0 + contrast) / 100.0;
                contrastFactor *= contrastFactor;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;
                        byte b = pixels[index]; byte g = pixels[index + 1]; byte r = pixels[index + 2];

                        // 대비 적용
                        double newR = ((r / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newG = ((g / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;
                        double newB = ((b / 255.0 - 0.5) * contrastFactor + 0.5) * 255.0;

                        // 밝기 적용
                        newR += brightness;
                        newG += brightness;
                        newB += brightness;

                        // 0-255 범위로 값 제한
                        pixels[index] = Clamp(newB);
                        pixels[index + 1] = Clamp(newG);
                        pixels[index + 2] = Clamp(newR);
                    }
                }
                wBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                return wBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyEffects Error: {ex.Message}");
                return originalImage; // 실패 시 원본 반환
            }
        }

        /// <summary>
        /// 값을 0-255(byte) 범위로 제한합니다.
        /// (DefectImageView.xaml.cs의 Clamp 로직 이동)
        /// </summary>
        private byte Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        // ⭐️ --- [추가 끝] ---
    }
}