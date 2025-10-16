using System;
using System.Globalization;
using System.IO;
using System.Linq;
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

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // 📁 Header Information
                if (line.StartsWith("FileVersion"))
                {
                    klarf.FileVersion = GetValue(line);
                }
                else if (line.StartsWith("ResultTimestamp"))
                {
                    klarf.ResultTimestamp = ParseDate(line);
                }
                else if (line.StartsWith("InspectionStationID"))
                {
                    klarf.InspectionStationId = GetValue(line);
                }
                else if (line.StartsWith("SampleType"))
                {
                    klarf.SampleType = GetValue(line);
                }
                else if (line.StartsWith("LotID"))
                {
                    klarf.LotId = GetValue(line);
                }
                else if (line.StartsWith("WaferID"))
                {
                    klarf.WaferId = GetValue(line);
                }
                else if (line.StartsWith("Slot"))
                {
                    klarf.Slot = int.Parse(line.Split(' ')[1].TrimEnd(';'));
                }
                else if (line.StartsWith("SampleSize"))
                {
                    klarf.WaferDiameter = double.Parse(line.Split(' ')[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("DiePitch"))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    klarf.DiePitchX = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    klarf.DiePitchY = double.Parse(parts[2].TrimEnd(';'), CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("AreaPerTest"))
                {
                    klarf.AreaPerTest = double.Parse(line.Split(' ')[1].TrimEnd(';'), CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("OrientationMarkLocation"))
                {
                    klarf.OrientationMarkLocation = line.Split(' ')[1].TrimEnd(';');
                }
                else if (line.StartsWith("TiffFilename"))
                {
                    klarf.TiffFileName = GetValue(line);
                }

                // 🧩 Sample Test Plan
                else if (line.StartsWith("SampleTestPlan"))
                {
                    inSampleTestPlan = true;
                }
                else if (line.StartsWith("EndOfSampleTestPlan"))
                {
                    inSampleTestPlan = false;
                }
                else if (inSampleTestPlan)
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
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
                else if (line.StartsWith("EndOfDefectList"))
                {
                    inDefectList = false;
                }
                else if (inDefectList)
                {
                    ParseDefectLine(line, klarf);
                }
            }

            klarf.IsParsed = true;
            return klarf;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 결함 데이터를 한 줄씩 파싱하여 KlarfModel에 추가합니다.
        /// </summary>
        private static void ParseDefectLine(string line, KlarfModel klarf)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 10)
                return;

            var defect = new Defect
            {
                DefectId = parts[0],
                XCoord = double.Parse(parts[1], CultureInfo.InvariantCulture),
                YCoord = double.Parse(parts[2], CultureInfo.InvariantCulture),
                Row = int.Parse(parts[3]),
                Column = int.Parse(parts[4]),
                Size = double.Parse(parts[8], CultureInfo.InvariantCulture),
                DefectType = parts[9]
            };

            klarf.Defects.Add(defect);
        }

        /// <summary>
        /// 문자열에서 따옴표로 감싼 값을 추출합니다.
        /// </summary>
        private static string GetValue(string line)
        {
            int start = line.IndexOf('"');
            int end = line.LastIndexOf('"');
            return (start >= 0 && end > start)
                ? line.Substring(start + 1, end - start - 1)
                : string.Empty;
        }

        /// <summary>
        /// KLARF의 날짜 문자열을 DateTime으로 변환합니다.
        /// </summary>
        private static DateTime ParseDate(string line)
        {
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length >= 2 && DateTime.TryParse(tokens[1], out DateTime date))
            {
                return date;
            }

            return DateTime.MinValue;
        }

        #endregion
    }
}
