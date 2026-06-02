using Microsoft.AspNetCore.SignalR;

namespace ChaoticCupidonServer
{

    public interface ICupidonClient
    {
        Task ReceiveLetter(LetterDTO letter);
    }

    public class LetterHub : Hub<ICupidonClient>
    {
        public static readonly Dictionary<string, Person> _persons = new();
        public static readonly HashSet<string> _usernames = new();

        private static readonly object _lock = new();

        public async Task InitSinglePerson(PersonDto personDto)
        {
            Person person;
            try
            {
                person = new(personDto.Username, personDto.City, personDto.Age, personDto.Phone, Context.ConnectionId);
            } catch (Exception e)
            {
                throw new HubException(e.Message, e);
            }
            lock (_lock)
            {
                if (_usernames.Contains(person.Username))
                    throw new HubException("Username already registered");
                if (_persons.ContainsKey(Context.ConnectionId))
                    throw new HubException($"User with connection id: {Context.ConnectionId} already connected");
                _persons[Context.ConnectionId] = person;
                _usernames.Add(person.Username);
            }

        }

        public async Task BlockUsername(string username)
        {
            lock (_lock)
            {
                if (_persons.TryGetValue(Context.ConnectionId, out Person person))
                {
                    person.BlockedUsernames[username] = new();
                }
            }
        }

        public async Task AcknowledgeLetter()
        {
            lock (_lock)
            {
                if (_persons.TryGetValue(Context.ConnectionId, out Person person)){
                    person.CanRecieveLetters = true;
                }
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                if (_persons.TryGetValue(Context.ConnectionId, out Person person))
                {
                    _persons.Remove(Context.ConnectionId);
                    _usernames.Remove(person.Username);
                }
                
            }
            Console.WriteLine($"User with id: {Context.ConnectionId} disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"User with id: {Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public static Dictionary<string, Person> GetPersons()
        {
            lock (_lock)
            {
                return new Dictionary<string, Person>(_persons);
            }
        }
    }
}
