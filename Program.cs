using PharmaConnect2;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

  ApiClient _apiClient;
  string _apiKey = "5E41A022A7BBB79874645GCFGK5F5A42FC6A43D837DCC94139DDCB29AE35C2B5";
  string _userId = "test44@naver.com";
  string _password = "test11!!";
  bool _isLoggedIn = false;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Console.Write("감시할 폴더 경로를 입력하세요: ");
string? directoryPath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
{
    Console.WriteLine("유효한 폴더 경로를 입력하세요.");
    return;
}

// 기존 파일들 먼저 처리
ProcessExistingFiles(directoryPath);

// 파일 시스템 감시 설정
FileSystemWatcher watcher = new FileSystemWatcher
{
    Path = directoryPath,
    Filter = "*.txt",
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
};

watcher.Created += (sender, e) => ProcessFile(e.FullPath);
watcher.Changed += (sender, e) => ProcessFile(e.FullPath);
watcher.EnableRaisingEvents = true;

Console.WriteLine("폴더를 감시 중입니다. 새로운 파일이 추가되거나 변경되면 자동으로 파싱됩니다...");
Console.ReadLine();

void ProcessExistingFiles(string path)
{
    string[] txtFiles = Directory.GetFiles(path, "*.txt");
    foreach (string file in txtFiles)
    {
        ProcessFile(file);
    }
}

void ProcessFile(string filePath)
{
    try
    {
        System.Threading.Thread.Sleep(1000); // 파일 저장 대기

        string content = File.ReadAllText(filePath, Encoding.GetEncoding("EUC-KR"));
        MatchCollection medicineMatches = Regex.Matches(content, @"\|JVPHEAD\|(.+?)\|JVPEND\|");

        if (medicineMatches.Count == 0)
        {
            Console.WriteLine($"파일 {filePath}에서 JVP 데이터를 찾을 수 없습니다.");
            return;
        }

        string[] splitContent = medicineMatches[0].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        PrescriptionDetail prescriptionDetail = new PrescriptionDetail();

        Match JVPmatch = Regex.Match(splitContent[0], @"\|JVPHEAD\|(\d+)");
        prescriptionDetail.patient_code = JVPmatch.Groups[1].Value;
        prescriptionDetail.receipt_number = int.Parse(splitContent[1]);
        prescriptionDetail.prescription_received_date = DateTime.ParseExact(splitContent[3].Substring(0, 13).Replace(":", ""), "yyyyMMddHHmm", null);
        prescriptionDetail.patient_birthdate = splitContent[4];
        prescriptionDetail.patient_gender = splitContent[3].Substring(19, 1);
        prescriptionDetail.patient_name = splitContent[5];

        var JVMmatch = Regex.Match(content, @"\|JVMHEAD\|(.*?)\|JVMEND\|", RegexOptions.Singleline);
        if (!JVMmatch.Success)
        {
            Console.WriteLine($"파일 {filePath}에서 JVM 데이터를 찾을 수 없습니다.");
            return;
        }


        //string[] medicineParts = Regex.Split(JVMmatch.Groups[1].Value, @"(?=T\d{9})")
        //                            .Where(x => !string.IsNullOrWhiteSpace(x)) // 빈 문자열 제거
        //                            .Select(x => x.Trim())
        //                            .ToArray();
        //string[] medicineParts = Regex.Split(JVMmatch.Groups[1].Value, @"(?=[A-Z][0-9]{8,9})")
        //                                .Where(x => !string.IsNullOrWhiteSpace(x))
        //                                .Select(x => x.Trim())
        //                                .ToArray();
        //string[] medicineParts = Regex.Split(JVMmatch.Groups[1].Value, @"(?=(?:[A-Z][0-9]{7,9}|[A-Z]{2}[0-9]{7,8}))")
        //                      .Where(x => !string.IsNullOrWhiteSpace(x))
        //                      .Select(x => x.Trim())
        //                      .ToArray();
        string[] medicineParts = Regex.Split(JVMmatch.Groups[1].Value, @"(?=(?:\b[A-Z]{1}[0-9]{8,9}\b|\b[A-Z]{2}[0-9]{7,8}\b))")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();
        foreach (string part in medicineParts)
        {
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }
            string[] JVMContent = Regex.Split(part, @"\s{3,}")
                                               .Where(x => !string.IsNullOrWhiteSpace(x)) // 빈 문자열 제거
                                               .ToArray();

            if (JVMContent.Length == 4)
            {
                prescriptionDetail.insurance_code = JVMContent[0];
                prescriptionDetail.medicine_name = JVMContent[1];
                prescriptionDetail.medicine_doctor_note = JVMContent[2];
                string[] parts = JVMContent[3]
               .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
               .Where(x => x.All(c => char.IsDigit(c) || c == '.')) // 숫자와 소수점만 포함된 항목 필터링
               .ToArray();

                string[] numericParts = parts
                 .SelectMany(x => Regex.Matches(x, @"\d+").Cast<Match>().Select(m => m.Value))
                 .ToArray();
                string doctorNote = string.Join(" ", parts.Where(x => !double.TryParse(x, out _)));
                prescriptionDetail.medicine_doctor_note = doctorNote;
                if(numericParts.Length == 1)
                {
                    prescriptionDetail.medicine_days_taken = int.Parse(numericParts[0].Substring(0, 3));
                    prescriptionDetail.medicine_dosage_count = int.Parse(numericParts[0].Substring(3, 1));
                    prescriptionDetail.medicine_dosage = float.Parse(numericParts[0].Substring(4));
                }
                else
                {
                    //prescriptionDetail.medicine_days_taken = int.Parse(parts[0]);
                    //prescriptionDetail.medicine_dosage_count = int.Parse(parts[1][0].ToString());
                    //prescriptionDetail.medicine_dosage = float.Parse(parts[1].Substring(1));
                    prescriptionDetail.medicine_days_taken = int.Parse(numericParts[0]);
                    prescriptionDetail.medicine_dosage_count = int.Parse(numericParts[1][0].ToString());
                    prescriptionDetail.medicine_dosage = float.Parse(numericParts[1].Substring(1));
                }

                
            }
            else
            {
                prescriptionDetail.insurance_code = JVMContent[0];
                prescriptionDetail.medicine_name = JVMContent[1];

                // 공백을 제거한 후, 숫자만 분리
                string[] parts = JVMContent[2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                //string[] numericParts = parts
                //   .SelectMany(x => Regex.Matches(x, @"\d+").Cast<Match>().Select(m => m.Value))
                //   .ToArray();
                string[] numericParts = parts
            .SelectMany(x => Regex.Matches(x, @"\d+(\.\d+)?").Cast<Match>().Select(m => m.Value))
            .ToArray();

                string doctorNote = string.Join(" ", parts.Where(x => !double.TryParse(x, out _)));
                prescriptionDetail.medicine_doctor_note = doctorNote;

                // 숫자만 필터링
                //string[] numericParts = parts.Where(x => double.TryParse(x, out _)).ToArray();

                // 문자만 필터링
                if (numericParts.Length == 1)
                {
                    prescriptionDetail.medicine_days_taken = int.Parse(numericParts[0].Substring(0,3));
                    prescriptionDetail.medicine_dosage_count = int.Parse(numericParts[0].Substring(3, 1));
                    prescriptionDetail.medicine_dosage = float.Parse(numericParts[0].Substring(4));
                }
                else if (numericParts.Length == 2)
                {
                    
                    // 값 대입
                    prescriptionDetail.medicine_days_taken = int.Parse(numericParts[0]);
                    prescriptionDetail.medicine_dosage_count = int.Parse(numericParts[1][0].ToString());
                    prescriptionDetail.medicine_dosage = float.Parse(numericParts[1].Substring(1));
                }


            }



            prescriptionDetail.PrintPrescriptionDetail();
            prescriptionDetail.AppendPrescriptionDetailToFile();
        }
    }
    catch (Exception ex)
    {
        //string destinationFolder = @"C:\Users\KIMO8\Downloads\Test5";

        Console.WriteLine($"파일 처리 중 오류 발생: {ex.Message}");
        //Console.WriteLine($"파일 처리 중 오류 발생: {filePath}");

        //Console.WriteLine(filePath);
        //string fileName = Path.GetFileName(filePath);
        //string destFile = Path.Combine(destinationFolder, fileName);
        //File.Move(filePath, destFile);
    }
}