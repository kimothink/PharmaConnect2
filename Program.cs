using PharmaConnect2;
using System.Text.RegularExpressions;
using System.Text;


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
        prescriptionDetail.prescribing_doctor = splitContent[7];

        var JVMmatch = Regex.Match(content, @"\|JVMHEAD\|(.*?)\|JVMEND\|", RegexOptions.Singleline);
        if (!JVMmatch.Success)
        {
            Console.WriteLine($"파일 {filePath}에서 JVM 데이터를 찾을 수 없습니다.");
            return;
        }

        string[] medicineParts = Regex.Split(JVMmatch.Groups[1].Value, @"\s{2,}T");

        foreach (string part in medicineParts)
        {
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            string[] JVMContent = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            prescriptionDetail.insurance_code = JVMContent[0];
            prescriptionDetail.medicine_name = JVMContent[1];
            prescriptionDetail.medicine_days_taken = int.Parse(JVMContent[2]);
            string lastElement = JVMContent[JVMContent.Length - 1];

            bool containsDot = lastElement.Contains(".");
            if (containsDot)
            {
                string[] parts = lastElement.Split('.');

                string beforeDot = parts[0];

                char lastChar = beforeDot.Last();
                prescriptionDetail.medicine_dosage = float.Parse(lastChar.ToString() + "." + parts[1]);
                prescriptionDetail.medicine_dosage_count = (int)(float.Parse(beforeDot) - prescriptionDetail.medicine_dosage);
            }
            else
            {
                prescriptionDetail.medicine_dosage_count = int.Parse(lastElement.Substring(0, 1));
                prescriptionDetail.medicine_dosage = float.Parse(lastElement.Substring(1, 1));
            }

            prescriptionDetail.PrintPrescriptionDetail();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"파일 처리 중 오류 발생: {ex.Message}");
    }
}