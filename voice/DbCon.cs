using System;
using System.Data.OleDb;

public static class DbCon
{
    // Строка подключения берется из App.config
    private static readonly string ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+ Environment.CurrentDirectory+ "\\DbVoice.accdb";

    // Метод для получения подключения к базе данных
    public static OleDbConnection GetConnection()
    {
        return new OleDbConnection(ConnectionString);
    }
}
