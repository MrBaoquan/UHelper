using System.IO;
using UnityEngine;
using System;
using System.Linq;
using NLog;

namespace UHelper
{
    
public static class ULogger
{
    const string configName = "NLog.config.xml";
    const string logFileName = "${shortdate}.log";
    
    static string LogFileDir{
        get{
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs");
        }
    }
    static string LogFilePath {
        get{
            return Path.Combine(LogFileDir, logFileName);
        }
    }
    static string configPath{
        get{
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,configName);
        }
    }

    private static NLog.Logger _logger = null;
    private static NLog.Logger logger {
        get{
            if(_logger==null){
                _logger = NLog.LogManager.GetCurrentClassLogger();
            }
            return _logger;
        }
    }

    public static void Debug(string InMessage){   
#if UNITY_EDITOR
        UnityEngine.Debug.Log(InMessage);
#endif
        logger.Debug(InMessage);
    }

    public static void Warning(string InMessage){   
#if UNITY_EDITOR
        UnityEngine.Debug.LogWarning(InMessage);
#endif
        logger.Warn(InMessage);
    }

    public static void Error(string InMessage){   
#if UNITY_EDITOR
        UnityEngine.Debug.LogError(InMessage);
#endif
        logger.Error(InMessage);
    }

    public static void Error(Exception InEx, string InMessage=""){   
#if UNITY_EDITOR
        UnityEngine.Debug.LogError(InMessage);
#endif
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
