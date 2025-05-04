using apbd_cw7.Exceptions;
using apbd_cw7.Models;
using apbd_cw7.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw7;

public interface IDbService
{
    Task<IEnumerable<TripWithCountryDTO>> GetAllTripsAsync();
    Task<IEnumerable<ClientWithTripDTO>> GetTripByIdAsync(int id);
    
    Task<ClientGetDTO> CreatClientByIdAsync(ClientCreateDTO client);
    
    Task<IEnumerable<ClientGetDTO>> GetAllClientsAsync();
    
    Task<ClientTripDTO> GetClientTripByIdsAsync(int clientId, int tripId);
    
    Task DeleteClientTripByIdsAsync(int clientId, int tripId);
}

public class DbService(IConfiguration config) : IDbService
{
   public async Task<IEnumerable<TripWithCountryDTO>> GetAllTripsAsync()
{
    var result = new List<TripWithCountryDTO>();
    var connectionString = config.GetConnectionString("Default");
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

 
    var tripSql = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
    var trips = new List<TripGetDTO>();

    await using (var tripCommand = new SqlCommand(tripSql, connection))
    await using (var reader = await tripCommand.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            var trip = new TripGetDTO()
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = Convert.ToString(reader.GetDateTime(3)),
                DateTo = Convert.ToString(reader.GetDateTime(4)),
                MaxPeople = reader.GetInt32(5)
            };
            trips.Add(trip);
        }
    }
    
    foreach (var trip in trips)
    {
        var countries = new List<string>();
        var countriesSql = "SELECT Country.Name FROM Country INNER JOIN Country_Trip ON Country.IdCountry = Country_Trip.IdCountry WHERE Country_Trip.IdTrip = @IdTrip";

        await using (var countriesCommand = new SqlCommand(countriesSql, connection))
        {
            countriesCommand.Parameters.AddWithValue("@IdTrip", trip.IdTrip);
            await using var reader = await countriesCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                countries.Add(reader.GetString(0));
            }
        }
        
        result.Add(new TripWithCountryDTO()
        {
            Trip = trip,
            Countries = countries
        });
    }

    return result;
}



    public async Task<IEnumerable<ClientWithTripDTO>> GetTripByIdAsync(int id)
    {
        var result = new List<ClientWithTripDTO>();
    var connectionString = config.GetConnectionString("Default");

    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    var checkClientSql = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
    await using (var checkClientCommand = new SqlCommand(checkClientSql, connection))
    {
        checkClientCommand.Parameters.AddWithValue("@IdClient", id);
        var clientExists = await checkClientCommand.ExecuteScalarAsync();
        if (clientExists == null)
        {
            throw new NotFoundException($"Client with id {id} does not exist.");
        }
    }
    
    var tripSql = "SELECT Trip.IdTrip, Name, Description, DateFrom, DateTo, MaxPeople, RegisteredAt, PaymentDate " +
                  "FROM Trip " +
                  "INNER JOIN Client_Trip ON Client_Trip.IdTrip= Trip.IdTrip " +
                  "INNER JOIN Client ON Client_Trip.IdClient= Client.IdClient " +
                  "WHERE Client.IdClient= @IdClient";

    await using var tripCommand = new SqlCommand(tripSql, connection);
    tripCommand.Parameters.AddWithValue("@IdClient", id);
    await using var reader = await tripCommand.ExecuteReaderAsync();

    if (!reader.HasRows)
    {
        throw new NotFoundException($"No trips found for client {id}.");
    }

    while (await reader.ReadAsync())
    {
        TripGetDTO trip = new TripGetDTO()
        {
            IdTrip = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            DateFrom = Convert.ToString(reader.GetDateTime(3)),
            DateTo = Convert.ToString(reader.GetDateTime(4)),
            MaxPeople = reader.GetInt32(5)
        };

        ClientWithTripDTO clientWithTrip = new ClientWithTripDTO()
        {
            Trip = trip,
            RegisteredAt = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
            PaymentDate = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
        };

        result.Add(clientWithTrip);
    }

    return result;

    }

    public async Task<ClientGetDTO> CreatClientByIdAsync(ClientCreateDTO client)
    {
  
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = "INSERT INTO Client (FirstName,LastName, Email, Telephone, Pesel) values (@FirstName, @LastName, @Email, @Telephone, @Pesel); select scope_identity();"; 
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        await connection.OpenAsync();
        
        var newid = Convert.ToInt32( await command.ExecuteScalarAsync());

        return new ClientGetDTO()
        {
            IdClient = newid,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }

    public async Task<IEnumerable<ClientGetDTO>> GetAllClientsAsync()
    {
        var result = new List<ClientGetDTO>();
        var connectionString = config.GetConnectionString("Default");
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel from Client";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            ClientGetDTO client = new ClientGetDTO()
            {
                IdClient= reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                Telephone = reader.GetString(4),
                Pesel = reader.GetString(5),
                
            };
            
            result.Add(client);
        }
        return result;
    }

public async Task<ClientTripDTO> GetClientTripByIdsAsync(int clientId, int tripId)
{
    var connectionString = config.GetConnectionString("Default");

    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    var checkClientSql = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
    await using (var checkClientCommand = new SqlCommand(checkClientSql, connection))
    {
        checkClientCommand.Parameters.AddWithValue("@IdClient", clientId);
        var clientExists = await checkClientCommand.ExecuteScalarAsync();
        if (clientExists == null)
        {
            throw new NotFoundException($"Client with id {clientId} does not exist.");
        }
    }
    
    var maxPeople = 0;
    var checkTripSql = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
    await using (var checkTripCommand = new SqlCommand(checkTripSql, connection))
    {
        checkTripCommand.Parameters.AddWithValue("@IdTrip", tripId);

        await using var reader = await checkTripCommand.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            throw new NotFoundException($"Trip with id {tripId} does not exist.");
        }

        await reader.ReadAsync();
        if (reader["MaxPeople"] == DBNull.Value)
        {
            throw new InvalidOperationException("MaxPeople is NULL for the given trip.");
        }

        maxPeople = Convert.ToInt32(reader["MaxPeople"]);
    }
    
    var numberOfRegisteredClientsSql = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
    await using (var numberOfRegisteredClientsCommand = new SqlCommand(numberOfRegisteredClientsSql, connection))
    {
        numberOfRegisteredClientsCommand.Parameters.AddWithValue("@IdTrip", tripId);
        var numberOfClients = (int)await numberOfRegisteredClientsCommand.ExecuteScalarAsync();

        if (numberOfClients >= maxPeople)
        {
            throw new InvalidOperationException($"The trip with id {tripId} is fully booked. Maximum allowed participants: {maxPeople}, Registered: {numberOfClients}.");
        }
    }
    
    var insertClientTripSql = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate) VALUES (@IdClient, @IdTrip, @RegisteredAt, @PaymentDate)";
    await using (var insertClientTripCommand = new SqlCommand(insertClientTripSql, connection))
    {
        insertClientTripCommand.Parameters.AddWithValue("@IdClient", clientId);
        insertClientTripCommand.Parameters.AddWithValue("@IdTrip", tripId);

        int registeredAtInt = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
        insertClientTripCommand.Parameters.AddWithValue("@RegisteredAt", registeredAtInt);
        insertClientTripCommand.Parameters.AddWithValue("@PaymentDate", DBNull.Value);

        await insertClientTripCommand.ExecuteNonQueryAsync();
    }
    
    return new ClientTripDTO
    {
        IdClient = clientId,
        IdTrip = tripId,
        RegisteredAt = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))
    };
}

public async Task DeleteClientTripByIdsAsync(int clientId, int tripId)
{
    var connectionString = config.GetConnectionString("Default");

    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    var checkClientSql = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
    await using var checkClientCommand = new SqlCommand(checkClientSql, connection);
    checkClientCommand.Parameters.AddWithValue("@IdClient", clientId);
    checkClientCommand.Parameters.AddWithValue("@IdTrip", tripId);
    var clientExists = await checkClientCommand.ExecuteScalarAsync();
    
    if (clientExists == null)
    {
        throw new NotFoundException($"Client with id {clientId} is not registered for trip {tripId}.");
    }
    
    var deleteClientTripSql = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
    await using var deleteClientTripCommand = new SqlCommand(deleteClientTripSql, connection);
    deleteClientTripCommand.Parameters.AddWithValue("@IdClient", clientId);
    deleteClientTripCommand.Parameters.AddWithValue("@IdTrip", tripId);
    
    int rowsAffected = await deleteClientTripCommand.ExecuteNonQueryAsync();

    if (rowsAffected > 0)
    {
        Console.WriteLine($"Client with id {clientId} has been successfully removed from trip {tripId}.");
    }
    else
    {
        throw new InvalidOperationException($"Failed to remove client with id {clientId} from trip {tripId}.");
    }
}

}