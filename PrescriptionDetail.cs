using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaConnect2
{
    public class PrescriptionDetail
    {
        /// <summary>
        /// 환자 코드
        /// </summary>
        public string patient_code { get; set; } = string.Empty;

        /// <summary>
        /// 접수 번호
        /// </summary>
        public int receipt_number { get; set; }


        /// <summary>
        /// 처방접수일
        /// </summary>
        public DateTime prescription_received_date { get; set; }

        /// <summary>
        /// 환자 주민번호_생년월일
        /// </summary>
        public string patient_birthdate { get; set; } = string.Empty;

        /// <summary>
        /// 환자 성별
        /// </summary>
        public string patient_gender { get; set; } = string.Empty;

        /// <summary>
        /// 환자 성함
        /// </summary>
        public string patient_name { get; set; } = string.Empty;

        /// <summary>
        /// 처방 기관
        /// </summary>
        public string prescription_institution { get; set; } = string.Empty;

    

        /// <summary>
        /// 보험 코드
        /// </summary>
        public string insurance_code { get; set; } = string.Empty;

        /// <summary>
        /// 의약품 명
        /// </summary>
        public string medicine_name { get; set; } = string.Empty;

        /// <summary>
        /// 의약품에 대한 의사 메모
        /// </summary>
        public string medicine_doctor_note { get; set; } = string.Empty;

        /// <summary>
        /// 의약품 복용 일수
        /// </summary>
        public int medicine_days_taken { get; set; }

        /// <summary>
        /// 복용 횟수
        /// </summary>
        public int medicine_dosage_count { get; set; }

        /// <summary>
        /// 투여량
        /// </summary>
        public float medicine_dosage { get; set; }

        public void PrintPrescriptionDetail()
        {
            Console.WriteLine("===== 처방전 상세 정보 =====");
            Console.WriteLine($"환자 코드: {patient_code}");
            Console.WriteLine($"접수 번호: {receipt_number}");
            Console.WriteLine($"처방 접수일: {prescription_received_date:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"환자 생년월일: {patient_birthdate}");
            Console.WriteLine($"환자 성별: {patient_gender}");
            Console.WriteLine($"환자 성명: {patient_name}");
            Console.WriteLine($"보험 코드: {insurance_code}");
            Console.WriteLine($"의약품 명: {medicine_name}");
            Console.WriteLine($"의약품에 대한 의사 메모: {medicine_doctor_note}");
            Console.WriteLine($"의약품 복용 일수: {medicine_days_taken}");
            Console.WriteLine($"복용 횟수: {medicine_dosage_count}");
            Console.WriteLine($"투여량: {medicine_dosage}");
            Console.WriteLine("============================");
        }
        public void AppendPrescriptionDetailToFile()
        {
            string fileName = "처방전_상세정보.txt";
            using (StreamWriter writer = new StreamWriter(fileName, true)) // true = append mode
            {
                writer.WriteLine("===== 처방전 상세 정보 =====");
                writer.WriteLine($"환자 코드: {patient_code}");
                writer.WriteLine($"접수 번호: {receipt_number}");
                writer.WriteLine($"처방 접수일: {prescription_received_date:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"환자 생년월일: {patient_birthdate}");
                writer.WriteLine($"환자 성별: {patient_gender}");
                writer.WriteLine($"환자 성명: {patient_name}");
                writer.WriteLine($"보험 코드: {insurance_code}");
                writer.WriteLine($"의약품 명: {medicine_name}");
                writer.WriteLine($"의약품에 대한 의사 메모: {medicine_doctor_note}");
                writer.WriteLine($"의약품 복용 일수: {medicine_days_taken}");
                writer.WriteLine($"복용 횟수: {medicine_dosage_count}");
                writer.WriteLine($"투여량: {medicine_dosage}");
                writer.WriteLine("============================");
                writer.WriteLine(); // 줄 바꿈
            }
        }
    }
}
