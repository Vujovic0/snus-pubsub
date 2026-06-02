using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;

namespace ChaoticCupidonServer
{
    public class LetterService : BackgroundService
    {
        private readonly IHubContext<LetterHub, ICupidonClient> _hubContext;
        private readonly RNGCryptoServiceProvider _numberGenerator = new();
        private readonly object _lock = new();
        private readonly string[] _messages = new[]
        {
            "Radujem se nasem susretu!",
            "Zelim da se upoznamo.",
            "Nisam zainteresovan/a za upoznavanje."
        };

        public LetterService(IHubContext<LetterHub, ICupidonClient> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(60 * 1000);
                Console.WriteLine($"[{DateTime.Now}]Sending letters...");
                await SendLetters();
            }
        }

        private async Task SendLetters()
        {
            var users = LetterHub.GetPersons().Values.ToList();

            if (users.Count < 2)
                return;

            var tasks = users.Select(sender => ProcessSender(sender, users));

            await Task.WhenAll(tasks);
        }

        private async Task ProcessSender(Person sender, List<Person> users)
        {
            var bestMatch = FindBestMatch(sender, users);

            if (bestMatch == null)
                return;

            MarkWaiting(bestMatch);
            var messageData = GenerateMessage(sender);

            await SendLetter(bestMatch, sender, messageData);
        }

        private async Task SendLetter(Person receiver, Person sender, (string message, string phone) data)
        {
            LetterDTO letter = new(data.message, sender.City, sender.Age, data.phone, sender.Username);
            await _hubContext.Clients.Client(receiver.ConnectionId)
                .ReceiveLetter(letter);
        }

        private Person? FindBestMatch(Person sender, List<Person> users)
        {
            Person? best = null;
            int bestScore = -1;

            foreach (var receiver in users)
            {
                if (sender.ConnectionId == receiver.ConnectionId) continue;
                if (!receiver.CanRecieveLetters) continue;
                if (receiver.BlockedUsernames.ContainsKey(sender.Username)) continue;

                int score = CalculateScore(sender, receiver);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = receiver;
                }
            }

            return best;
        }

        private void MarkWaiting(Person person)
        {
            person.CanRecieveLetters = false;
        }

        private (string message, string phone) GenerateMessage(Person sender)
        {
            byte[] bytes = new byte[1];
            const int limit = 256 - (256 % 3);
            byte value;
            do
            {
                _numberGenerator.GetBytes(bytes);
                value = bytes[0];
            }
            while (value >= limit);
            int index = value % 3;
            string message = _messages[index];

            string phone = message == "Nisam zainteresovan/a za upoznavanje."
                ? "N/A"
                : sender.Phone;

            return (message, phone);
        }

        private int GetRandomScore()
        {
            byte[] bytes = new byte[1];

            const int limit = 256 - (256 % 101);

            byte value;

            using (var rng = new RNGCryptoServiceProvider())
            {
                do
                {
                    rng.GetBytes(bytes);
                    value = bytes[0];
                }
                while (value >= limit);
            }

            return value % 101;
        }

        private int CalculateScore(Person sender, Person receiver)
        {
            int score = 0;
            if (sender.City.Equals(receiver.City, StringComparison.OrdinalIgnoreCase))
            {
                score += 30;
            }

            if (Math.Abs(sender.Age - receiver.Age) <= 2)
            {
                score += 20;
            }

            score += GetRandomScore();

            return score;
        }
    }
}
