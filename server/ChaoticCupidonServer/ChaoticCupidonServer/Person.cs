using System.Collections.Concurrent;

namespace ChaoticCupidonServer
{
    public class Person
    {
        public string ConnectionId { get; set; }
        public String Username { get; set; }
        public String City { get; set; }
        public int Age { get; set; }
        public String Phone { get; set; }
        public Boolean CanRecieveLetters { get; set; }
        public ConcurrentDictionary<String, byte> BlockedUsernames { get; set; }

        public Person(string username, string city, int age, string phone, string connectionId)
        {
            Username = username.Trim();
            City = city.Trim();
            Age = age;
            Phone = phone.Trim();
            Validate(Username, City, Age, Phone);

            ConnectionId = connectionId;
            CanRecieveLetters = true;
            BlockedUsernames = new();
        }

        private void Validate(String username, String city, int age, String phone)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must not be blank");
            if (age < 0)
                throw new ArgumentException("User age can not be negative");
            if (age < 18)
                throw new ArgumentException("User must be at least 18 years old");
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone number must not be blank");
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City must not be blank");
        }
    }
}
