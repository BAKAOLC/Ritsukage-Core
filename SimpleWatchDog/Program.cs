using SimpleWatchDog;
using System.Diagnostics;
using static SimpleWatchDog.Console.ConsoleLog;
using static System.Console;

const string ProgressName = "Watch Dog";

void LogDebug(string message) => Debug(ProgressName, message);
void LogInfo(string message) => Info(ProgressName, message);
void LogWarning(string message) => Warning(ProgressName, message);
void LogError(string message) => Error(ProgressName, message);

ushort PID = 0;
string PipeName = "SimpleWatchDog";
TimeSpan TimeoutTimeSpan = default;
bool ReceivedFirstHeartBeat = false;
DateTime LastHeartBeatTime = default;

bool Working = false;
bool Watching = false;
SimpleIPC.Server PipeServer;
Process WatchedProcess = null;

if (SolveArgs(args))
{
    Init();
    while (Working || Watching)
        Thread.Sleep(1000);
}

void Init()
{
    CatchProcess();
    if (WatchedProcess != null)
    {
        Working = true;
        CreatePipeServer();
        CreateWatchThread();
    }
}

void CatchProcess()
{
    try
    {
        WatchedProcess = Process.GetProcessById(PID);
    }
    catch
    {
        LogError("未能查找到指定PID的程序");
    }
}

void CreatePipeServer()
{
    LogInfo($"创建通信管道 {PipeName}");
    PipeServer = new(PipeName);
    PipeServer.MessageReceived += (sender, msg) =>
    {
        LogDebug("收到心跳消息");
        if (!ReceivedFirstHeartBeat)
            ReceivedFirstHeartBeat = true;
        LastHeartBeatTime = DateTime.Now;
    };
}

void CreateWatchThread()
{
    LogInfo($"创建心跳线程");
    Watching = true;
    new Thread(() =>
    {
        while (!ReceivedFirstHeartBeat)
            Thread.Sleep(100);
        while (Working)
        {
            if (WatchedProcess.HasExited || (DateTime.Now - LastHeartBeatTime) > TimeoutTimeSpan)
                Working = false;
            else
                Thread.Sleep(100);
        }
        if (!WatchedProcess.HasExited)
        {
            LogWarning("心跳已超时，尝试停止目标进程");
            try
            {
                KillProgress(WatchedProcess.Id);
                //WatchedProcess.Kill(true);
            }
            catch
            {
                LogError("进程停止操作执行失败");
            }
        }
        Watching = false;
    })
    {
        IsBackground = true
    }.Start();
}

bool SolveArgs(string[] args)
{
    ArgsResolver ar;
    try
    {
        ar = new(new(args));
    }
    catch (Exception ex)
    {
        WriteLine(ex.Message + " 使用 -h 获取更多信息");
        return false;
    }
    if (ar.IsHelp || ar.PID == null)
    {
        WriteLine("SimpleWatchDog [progressPid][-n pipeName][-d heartBeatDuration][-h]");
        WriteLine();
        WriteLine("\t-p\tprogressPid\t\t要监视的进程PID");
        WriteLine();
        WriteLine("\t-n\tpipeName\t\t通信用的管道名称");
        WriteLine("\t\t\t\t\t默认为 'SimpleWatchDog'");
        WriteLine();
        WriteLine("\t-d\theartBeatDuration\t如果该项被给出，将会用其作为心跳超时的时长（秒）");
        WriteLine("\t\t\t\t\t默认为 30");
        WriteLine();
        WriteLine("\t-h\t\t\t\t显示帮助");
        WriteLine();
        return false;
    }
    PipeName = ar.PipeName;
    PID = (ushort)ar.PID;
    TimeoutTimeSpan = TimeSpan.FromSeconds(ar.Duration ?? 30);
    return true;
}

void KillProgress(int pid)
{
    Process p = new();
    p.StartInfo.FileName = "cmd.exe";
    p.StartInfo.Arguments = $"/c taskkill /pid {pid} -t -f";
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardInput = true;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.RedirectStandardError = true;
    p.StartInfo.CreateNoWindow = true;
    p.Start();
}