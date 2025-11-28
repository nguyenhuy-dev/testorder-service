namespace TestOrderService.API.DTOs.AIReview
{
    public class AIReviewRequestDto
    {
        public int Sex { get; set; } // 0=Male, 1=Female
        public double Wbc { get; set; } // x10^9/L
        public double Rbc { get; set; } // x10^12/L
        public double Hgb { get; set; } // g/dL
        public double Hct { get; set; } // %
        public double Plt { get; set; } // x10^9/L
        public double Mcv { get; set; } // fL
        public double Mch { get; set; } // pg
        public double Mchc { get; set; } // g/dL
    }
}
