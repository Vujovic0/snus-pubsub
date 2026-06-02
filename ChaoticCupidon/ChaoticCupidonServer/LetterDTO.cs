namespace ChaoticCupidonServer
{
    public class LetterDTO
    {
        public String Message { get; set; }
        public String City { get; set; }
        public int Age { get; set; }
        public String Phone { get; set; }
        public String Username {  get; set; }

        public LetterDTO(String message, String city, int age, string phone, string username)
        {
            Message = message;
            City = city;
            Age = age;
            Phone = phone;
            Username = username;
        }
    }
}
