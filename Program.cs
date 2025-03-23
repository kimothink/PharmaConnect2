using PharmaConnect2;
using System.Text;
using System.Text.RegularExpressions;


Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


string directoryPath = @"C:\\Users\\kimo8\\Downloads\\test";
while (true)
{
    string[] txtFiles = Directory.GetFiles(directoryPath, "*.txt");
    PrescriptionDetail prescriptionDetail = new PrescriptionDetail();
    foreach (string file in txtFiles)
    {
        string content = File.ReadAllText(file, Encoding.GetEncoding("EUC-KR"));
        MatchCollection medicineMatches = Regex.Matches(content, @"\|JVPHEAD\|(.+?)\|JVPEND\|");


        string[] splitContent = medicineMatches[0].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        Match JVPmatch = Regex.Match(splitContent[0], @"\|JVPHEAD\|(\d+)");
        prescriptionDetail.patient_code = JVPmatch.Groups[1].Value;
        prescriptionDetail.receipt_number = int.Parse(splitContent[1]);
        prescriptionDetail.prescription_received_date = DateTime.ParseExact(splitContent[3].Substring(0, 13).Replace(":", ""), "yyyyMMddHHmm", null);
        prescriptionDetail.patient_birthdate = splitContent[4];
        prescriptionDetail.patient_gender = splitContent[3].Substring(19, 1);
        prescriptionDetail.patient_name = splitContent[5];
        prescriptionDetail.prescribing_doctor = splitContent[7];

        var JVMmatch = Regex.Match(content, @"\|JVMHEAD\|(.*?)\|JVMEND\|", RegexOptions.Singleline);
        //string[] extractedData = JVMmatch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
                string[] parts = lastElement.Split('.'); // '.'을 기준으로 분리

                string beforeDot = parts[0]; // '.' 앞의 문자열

                // 'beforeDot'이 10 또는 100일 경우 마지막 자리의 값만 가져오기
                char lastChar = beforeDot.Last(); // 마지막 문자 가져오기
                prescriptionDetail.medicine_dosage = float.Parse(lastChar.ToString() + "." + parts[1]);
                prescriptionDetail.medicine_dosage_count = (int)(float.Parse(beforeDot) - prescriptionDetail.medicine_dosage);

            }
            else
            {
                prescriptionDetail.medicine_dosage_count = int.Parse(lastElement.Substring(0, 1));  // 첫 번째 문자
                prescriptionDetail.medicine_dosage = float.Parse(lastElement.Substring(1, 1)); // 두 번째 문자
                prescriptionDetail.PrintPrescriptionDetail();
            }

        }

        prescriptionDetail = new PrescriptionDetail();
    }
    System.Threading.Thread.Sleep(5000); // 5초마다 폴더 확인
}