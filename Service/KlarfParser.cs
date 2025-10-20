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
                    klarf.InspectionStationId = GetQuotedValue(line);
                }
                else if (line.StartsWith("SampleType"))
                {
                    klarf.SampleType = line.Split(' ')[1].TrimEnd(';');
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

                        // 다음 줄이 숫자로 시작하면 defect 완료
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
        /// 결함 데이터를 파싱하여 KlarfModel에 추가합니다.
        /// DefectList 형식: DEFECTID XREL YREL XINDEX YINDEX XSIZE YSIZE DEFECTAREA DSIZE CLASSNUMBER ...
        /// </summary>
        private static void ParseDefectLine(string line, KlarfModel klarf)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 10)
                return;

            try
            {
                var defect = new Defect
                {
                    DefectId = parts[0],
                    XCoord = double.Parse(parts[1], CultureInfo.InvariantCulture),
                    YCoord = double.Parse(parts[2], CultureInfo.InvariantCulture),
                    Row = int.Parse(parts[3]),
                    Column = int.Parse(parts[4]),
                    Size = double.Parse(parts[8], CultureInfo.InvariantCulture),
                    DefectType = parts.Length > 9 ? parts[9] : "0"
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
                    new[] { "MM-dd-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" },
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