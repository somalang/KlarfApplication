using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using KlarfApplication.Model;

namespace KlarfApplication.Service
{
    /// <summary>
    /// KLARF 파일을 파싱하여 KlarfModel에 데이터를 채워 넣는 서비스 클래스입니다.
    /// </summary>
    public class KlarfParser
    {
        #region Public Methods

        /// <summary>
        /// 지정된 KLARF 파일을 읽어 KlarfModel 객체로 변환합니다.
        /// </summary>
        /// <param name="filePath">KLARF 파일 경로</param>
        /// <returns>파싱된 KlarfModel 객체</returns>
        public KlarfModel Parse(string filePath)
        {
            var klarf = new KlarfModel
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileDate = File.GetLastWriteTime(filePath),
                FileExtension = Path.GetExtension(filePath)
            };

            var lines = File.ReadAllLines(filePath);
            bool inSampleTestPlan = false;
            bool inDefectRecordSpec = false;
            bool inDefectList = false;
            int sampleTestPlanCount = 0;
            StringBuilder defectLineBuffer = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // 📁 Header Information
                if (line.StartsWith("FileVersion"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.FileVersion = $"{parts[1]} {parts[2].TrimEnd(';')}";
                    }
                }
                else if (line.StartsWith("ResultTimestamp"))
                {
                    klarf.ResultTimestamp = ParseTimestamp(line);
                }
                else if (line.StartsWith("InspectionStationID"))
                {
                    var stationIds = System.Text.RegularExpressions.Regex.Matches(line, "\"(.*?)\"")
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Select(m => m.Groups[1].Value)
                        .Where(s => !string.IsNullOrEmpty(s));
                    klarf.InspectionStationId = string.Join(" / ", stationIds);
                }
                else if (line.StartsWith("SampleType"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.SampleType = parts[1].TrimEnd(';');
                    }
                }
                else if (line.StartsWith("LotID"))
                {
                    klarf.LotId = GetQuotedValue(line);
                }
                else if (line.StartsWith("WaferID"))
                {
                    klarf.WaferId = GetQuotedValue(line);
                }
                else if (line.StartsWith("Slot"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.Slot = int.Parse(parts[1].TrimEnd(';'));
                    }
                }
                else if (line.StartsWith("SampleSize"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.WaferDiameter = double.Parse(parts[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                    }
                }
                else if (line.StartsWith("DiePitch"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.DiePitchX = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        klarf.DiePitchY = double.Parse(parts[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                    }
                }
                else if (line.StartsWith("AreaPerTest"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.AreaPerTest = double.Parse(parts[1].TrimEnd(';'), CultureInfo.InvariantCulture);
                    }
                }
                else if (line.StartsWith("OrientationMarkLocation"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.OrientationMarkLocation = parts[1].TrimEnd(';');
                    }
                }
                else if (line.StartsWith("TiffFilename"))
                {
                    klarf.TiffFileName = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1).FirstOrDefault()?.TrimEnd(';') ?? "";
                }

                // [추가] 새로운 헤더 필드 파싱 시작
                else if (line.StartsWith("TiffSpec"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.TiffSpec = $"{parts[1]} {parts[2].TrimEnd(';')}";
                    }
                }
                else if (line.StartsWith("FileTimestamp"))
                {
                    klarf.FileTimestamp = ParseTimestamp(line);
                }
                else if (line.StartsWith("SetupID"))
                {
                    klarf.SetupId = GetQuotedValue(line); // "Recipe"
                }
                else if (line.StartsWith("StepID"))
                {
                    klarf.StepId = GetQuotedValue(line); // "Recipe"
                }
                else if (line.StartsWith("SampleOrientationMarkType"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.SampleOrientationMarkType = parts[1].TrimEnd(';');
                    }
                }
                else if (line.StartsWith("DieOrigin"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.DieOriginX = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        klarf.DieOriginY = double.Parse(parts[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                    }
                }
                else if (line.StartsWith("SampleCenterLocation"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        klarf.SampleCenterX = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        klarf.SampleCenterY = double.Parse(parts[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                    }
                }
                else if (line.StartsWith("InspectionTest"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        klarf.InspectionTest = int.Parse(parts[1].TrimEnd(';'));
                    }
                }
                // [추가] 새로운 헤더 필드 파싱 끝

                // 🧩 Sample Test Plan
                else if (line.StartsWith("SampleTestPlan"))
                {
                    inSampleTestPlan = true;
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int count))
                    {
                        sampleTestPlanCount = count;
                    }
                }
                else if (inSampleTestPlan)
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && parts[1].EndsWith(";"))
                    {
                        // 마지막 die (세미콜론이 있음)
                        klarf.DieMap.Add(new DieModel
                        {
                            Row = int.Parse(parts[0]),
                            Column = int.Parse(parts[1].TrimEnd(';'))
                        });
                        inSampleTestPlan = false;
                    }
                    else if (parts.Length == 2)
                    {
                        // 중간 die
                        klarf.DieMap.Add(new DieModel
                        {
                            Row = int.Parse(parts[0]),
                            Column = int.Parse(parts[1])
                        });
                    }
                }

                // 🧾 Defect Record Spec
                else if (line.StartsWith("DefectRecordSpec"))
                {
                    inDefectRecordSpec = true;
                    var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(2);
                    foreach (var field in fields)
                    {
                        klarf.DefectRecordSpec.Add(field.TrimEnd(';'));
                    }
                }

                // ⚠️ Defect List
                else if (line.StartsWith("DefectList"))
                {
                    inDefectList = true;
                }
                else if (inDefectList)
                {
                    if (line.EndsWith(";"))
                    {
                        // Defect 완료
                        defectLineBuffer.Append(line);
                        ParseDefectLine(defectLineBuffer.ToString(), klarf);
                        defectLineBuffer.Clear();
                        inDefectList = false;
                    }
                    else
                    {
                        // Defect 데이터 누적
                        defectLineBuffer.Append(line);
                        defectLineBuffer.Append(" ");

                        // 다음 줄이 숫자로 시작하면 defect 완료 (기존 로직 유지)
                        if (i + 1 < lines.Length)
                        {
                            var nextLine = lines[i + 1].Trim();
                            if (!string.IsNullOrWhiteSpace(nextLine) && char.IsDigit(nextLine[0]))
                            {
                                ParseDefectLine(defectLineBuffer.ToString(), klarf);
                                defectLineBuffer.Clear();
                            }
                        }
                    }
                }
            }

            klarf.IsParsed = true;
            return klarf;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// [수정] DefectRecordSpec 17... 에 맞게 파싱 로직을 변경합니다.
        /// 0:DEFECTID 1:XREL 2:YREL 3:XINDEX 4:YINDEX 5:XSIZE 6:YSIZE 7:DEFECTAREA
        /// 8:DSIZE 9:CLASSNUMBER 10:TEST 11:CLUSTERNUMBER 12:ROUGHBINNUMBER
        /// 13:FINEBINNUMBER 14:REVIEWSAMPLE 15:IMAGECOUNT 16:IMAGELIST
        /// </summary>
        private static void ParseDefectLine(string line, KlarfModel klarf)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // [수정] 필드 개수 17개로 변경
            if (parts.Length < 17)
                return;

            try
            {
                var defect = new Defect
                {
                    DefectId = parts[0],  // 0: DEFECTID
                    XCoord = double.Parse(parts[1], CultureInfo.InvariantCulture), // 1: XREL
                    YCoord = double.Parse(parts[2], CultureInfo.InvariantCulture), // 2: YREL
                    Row = int.Parse(parts[3]),    // 3: XINDEX
                    Column = int.Parse(parts[4]), // 4: YINDEX

                    // [추가]
                    XSize = double.Parse(parts[5], CultureInfo.InvariantCulture), // 5: XSIZE
                    YSize = double.Parse(parts[6], CultureInfo.InvariantCulture), // 6: YSIZE
                    DefectArea = double.Parse(parts[7], CultureInfo.InvariantCulture), // 7: DEFECTAREA

                    Size = double.Parse(parts[8], CultureInfo.InvariantCulture), // 8: DSIZE
                    DefectType = parts[9], // 9: CLASSNUMBER

                    // [추가]
                    Test = int.Parse(parts[10]), // 10: TEST
                    ClusterNumber = int.Parse(parts[11]), // 11: CLUSTERNUMBER
                    RoughBinNumber = int.Parse(parts[12]), // 12: ROUGHBINNUMBER
                    FineBinNumber = int.Parse(parts[13]), // 13: FINEBINNUMBER
                    ReviewSample = int.Parse(parts[14]), // 14: REVIEWSAMPLE
                    ImageCount = int.Parse(parts[15]), // 15: IMAGECOUNT
                    ImageList = parts[16].TrimEnd(';') // 16: IMAGELIST
                };

                klarf.Defects.Add(defect);
            }
            catch (Exception ex)
            {
                // 파싱 실패 시 무시 (로그에 기록할 수도 있음)
                System.Diagnostics.Debug.WriteLine($"Defect parsing error: {ex.Message}");
            }
        }

        /// <summary>
        /// 따옴표로 감싼 값을 추출합니다.
        /// 예: InspectionStationID "ATI" "WIND" "" → ATI
        /// </summary>
        private static string GetQuotedValue(string line)
        {
            int start = line.IndexOf('"');
            if (start < 0) return string.Empty;

            int end = line.IndexOf('"', start + 1);
            return (end > start) ? line.Substring(start + 1, end - start - 1) : string.Empty;
        }

        /// <summary>
        /// KLARF의 타임스탬프를 DateTime으로 변환합니다.
        /// 예: ResultTimestamp 08-18-2023 16:19:42;
        /// </summary>
        private static DateTime ParseTimestamp(string line)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                string dateStr = $"{parts[1]} {parts[2].TrimEnd(';')}";
                if (DateTime.TryParseExact(dateStr,
                    new[] { "MM-dd-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, // 다양한 형식 지원
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
                {
                    return result;
                }
            }
            return DateTime.MinValue;
        }

        #endregion
    }
}