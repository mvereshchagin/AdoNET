using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using WritableJsonConfiguration;
using Data;
using System.Data;

//// Build a config object, using env vars and JSON providers.
//IConfiguration config = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json")
//    .Build();

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
IConfigurationRoot config = configurationBuilder.Add<WritableJsonConfigurationSource>(
    (Action<WritableJsonConfigurationSource>)(s =>
    {
        s.FileProvider = null;
        s.Path = "appsettings.json";
        s.Optional = false;
        s.ReloadOnChange = true;
        s.ResolveFileProvider();
    })).Build();

string connectionString = config.GetConnectionString("Default");

SqlConnection conn = new SqlConnection(connectionString);

try
{
    conn.Open();
}
catch (Exception ex)
{
    Console.WriteLine("Can not access to database");
    Environment.Exit(0);
}

TeamList teams = new TeamList();
string cmdString = "SELECT * FROM Teams";
using (SqlCommand cmd = new SqlCommand(cmdString, conn))
using (SqlDataReader reader = cmd.ExecuteReader())
    while (reader.Read())
    {
        Team team = new Team
        {
            ID = Guid.Parse(reader["ID"].ToString()),
            Name = reader["Name"].ToString(),
            Country = reader["Country"].ToString(),
            Budget = reader["Budget"] != DBNull.Value ? (int)reader["Budget"] : null
        };
        teams.Add(team);
    }

foreach(var team in teams)
{
    Console.WriteLine($"{team.ID}, {team.Name}, {team.Country}");
}

var barcelona = teams["barcelona"];
barcelona.Budget = 1000;
string updateString = $"UPDATE Teams SET [Budget] = {barcelona.Budget} WHERE [ID] = '{barcelona.ID}'";
using (SqlCommand cmd = new SqlCommand(updateString, conn))
    try
    {
        cmd.ExecuteNonQuery();
    } 
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

DataSet dataSet = new DataSet("Tournaments");
DataTable dtTeams = new DataTable("Teams");
dtTeams.Columns.Add("ID", typeof(Guid));
dtTeams.Columns.Add("Name", typeof(string));
dtTeams.Columns.Add("Country", typeof(string));
dtTeams.Columns.Add("Budget", typeof(int));
dataSet.Tables.Add(dtTeams);


SqlDataAdapter adapter = 
    new SqlDataAdapter("SELECT [ID], [Name], [Country], [Budget] FROM [Teams]", conn);
adapter.Fill(dtTeams);

Console.WriteLine($"Amount of rows = {dataSet.Tables["Teams"].Rows.Count}");

foreach(DataRow row in dataSet.Tables["Teams"].Rows)
{
    Console.WriteLine($"{row["ID"]}, {row["Name"]}, {row["Country"]}, {row["Budget"]}");
    row["Budget"] = 5000;
}

Console.WriteLine("We are finished");



adapter.UpdateCommand = new SqlCommand("", conn);
adapter.UpdateCommand.CommandText = "UPDATE Teams SET [Budget] = @Budget WHERE [ID] = @ID";
adapter.UpdateCommand.Parameters.Add("Budget", System.Data.SqlDbType.Int);
adapter.UpdateCommand.Parameters.Add("ID", System.Data.SqlDbType.UniqueIdentifier);
adapter.UpdateCommand.Parameters["ID"].SourceColumn = "ID";
adapter.UpdateCommand.Parameters["Budget"].SourceColumn = "Budget";
adapter.Update(dtTeams);







