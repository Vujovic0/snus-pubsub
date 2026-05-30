using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace main
{
    public class Program
    {
        private static Boolean _hasUnreadLetter = false;
        public static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7299/letterHub")
                .WithAutomaticReconnect()
                .Build();

            connection.On<LetterDto>("ReceiveLetter", async letter =>
            {
                Console.WriteLine("\n=== LETTER ===");
                Console.WriteLine(letter.Message);
                Console.WriteLine($"City: {letter.City},\n" +
                    $"Age: {letter.Age}");

                if (letter.Phone != "N/A")
                    Console.WriteLine($"Phone: {letter.Phone}");

                Console.WriteLine($"Received at {DateTime.Now}");
                Console.WriteLine("==============\n");

                _hasUnreadLetter = true;
                Console.WriteLine("Type /acknowledge to mark as seen");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                return;
            }

            while (true)
            {
                string username = ReadNonEmpty("Username: ");
                string city = ReadNonEmpty("City: ");
                int age = ReadPositiveInt("Age: ");
                string phone = ReadNonEmpty("Phone: ");

                try
                {
                    await connection.InvokeAsync("InitSinglePerson", new PersonDto
                    {
                        Username = username,
                        City = city,
                        Age = age,
                        Phone = phone
                    });

                    Console.WriteLine("Successfully registered.");
                    break;
                }
                catch (HubException ex)
                {
                    Console.WriteLine($"Registration failed: {ex.Message}");
                    Console.WriteLine("Please try again.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    Console.WriteLine("Please try again.\n");
                }
            }

            Console.WriteLine("Ready. Commands: /block username\n" +
                "/acknowledge");

            while (true)
            {
                try
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.StartsWith("/block "))
                    {
                        var user = input.Substring(7).Trim();

                        if (string.IsNullOrWhiteSpace(user))
                        {
                            Console.WriteLine("Username must not be empty.");
                            continue;
                        }

                        await connection.InvokeAsync("BlockUsername", user);
                        Console.WriteLine($"Blocked {user}");
                    }
                    else if (input.Equals("/acknowledge", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!_hasUnreadLetter)
                        {
                            Console.WriteLine("There is no unread letter.");
                            continue;
                        }

                        await connection.InvokeAsync("AcknowledgeLetter");
                        _hasUnreadLetter = false;

                        Console.WriteLine("Letter acknowledged.");
                    }
                }
                catch (HubException ex)
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(input))
                    return input;

                Console.WriteLine("Input must not be empty");
            }
        }

        static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out int value) && value > 0)
                    return value;

                Console.WriteLine("Enter a positive number");
            }
        }
    }
}