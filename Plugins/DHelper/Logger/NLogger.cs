using System.IO;
using System;
using NLog;


namespace UNIHper
{
    

public static class NLogger
{
    const string configName = "NLog.config.xml";
    const string logFileName = "${shortdate}.log";

    private static string logFileDir = string.Empty;
    public static string LogFileDir{
        get{return logFileDir;}
        set{logFileDir=value;}
    }
    static string LogFilePath {
        get{
            return Path.Combine(LogFileDir, logFileName);
        }
    }

    static NLogger(){}

    private static NLog.Logger _logger = null;
    private static NLog.Logger logger {
        get{
            if(_logger==null){
                _logger = NLog.LogManager.GetCurrentClassLogger();
            }
            return _logger;
        }
    }


    public static void Debug(object InMessage){
        logger.Debug(InMessage);
    }
    
    public static void Debug(string InMessage){   
        logger.Debug(InMessage);
    }

    public static void Debug(string InFormat, params object[] InParams){
        logger.Debug(InFormat,InParams);
    }

    public static void Warn(string InMessage){
        logger.Warn(InMessage);
    }
    public static void Warn(string InFormat, params object[] InParams){
        logger.Warn(InFormat, InParams);
    }

    public static void Error(string InMessage){
        logger.Error(InMessage);
    }

    public static void Error(string InFormat, params object[] InParams){
        logger.Error(InFormat, InParams);
    }

    public static void Error(Exception InEx, string InMessage=""){
        logger.Error(InEx, InMessage);
    }

    

    public static void Initialize()
    {
        var config = new NLog.Config.LoggingConfiguration();
        var logfile = new NLog.Targets.FileTarget("logfile"){FileName=LogFilePath};
        logfile.ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.DateAndSequence;
        logfile.ArchiveFileName = Path.Combine(LogFileDir,"backup-{#}.log");
        logfile.MaxArchiveFiles = 10;
        logfile.ArchiveEvery = NLog.Targets.FileArchivePeriod.Day;
        logfile.Layout = "${longdate} ${level} ${message} ${exception:format=Message} ${exception:format=StackTrace:exceptionDataSeparator=\r\n}";
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
        NLog.LogManager.Configuration = config;   
    }

    public static void Uninitialize()
    {
        NLog.LogManager.Shutdown();
    }



}



}