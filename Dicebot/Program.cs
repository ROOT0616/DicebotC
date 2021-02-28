using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Dicebot
{
  class Program
  {
    private DiscordSocketClient _client;
    public static CommandService _commands;
    public static IServiceProvider _services;

    static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
      _client = new DiscordSocketClient(new DiscordSocketConfig
      {
        LogLevel = LogSeverity.Info
      });
      _client.Log += Log;
      _commands = new CommandService();
      _services = new ServiceCollection().BuildServiceProvider();
      _client.MessageReceived += CommandRecieved;
      //次の行に書かれているstring token = "hoge"に先程取得したDiscordTokenを指定する。
      string token = "";
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      await Task.Delay(-1);
    }

    /// <summary>
    /// 何かしらのメッセージの受信
    /// </summary>
    /// <param name="msgParam"></param>
    /// <returns></returns>
    private async Task CommandRecieved(SocketMessage messageParam)
    {
      var message = messageParam as SocketUserMessage;

      //デバッグ用メッセージを出力
      Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);
      //メッセージがnullの場合
      if (message == null)
        return;

      //発言者がBotの場合無視する
      if (message.Author.IsBot)
        return;
      var id = message.Author.Id;

      var context = new CommandContext(_client, message);

      //ここから記述--------------------------------------------------------------------------
      var CommandContext = message.Content;
      string DiceNum;//ダイスの面の数
      string NumTimes;//ダイスを振る回数
      string ClearNum;//危険度
      string ClearTimes;//難易度
      int seed = Environment.TickCount;

      int Random_basic(string DiceNum)
      {
        Random rnd = new Random(seed++);
        int m1 = rnd.Next(1, int.Parse(DiceNum) + 1);
        return m1;
      }
      int List_Sum(List<int> list1)
      {
        int num = 0;
        for (int i = 0; i < list1.Count; i++)
        {
          num += list1[i];
        }
        return num;
      }

      // コマンド("!d")かどうか判定
      if (CommandContext.Substring(0, 2) == "!d")
      {

        if (CommandContext.Substring(2) == "")
        {
          await message.Channel.SendMessageAsync("説明を");
        }

        if (CommandContext.Substring(2) != "")
        {
          if (!(CommandContext.IndexOf("n") - 2 > 0))
            return;

          NumTimes = CommandContext.Substring(2, CommandContext.IndexOf("n") - 2);
          if (!(int.Parse(NumTimes) < 11))
          {
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "10回以下でお願いします");
          }
          else
          {
            //DiceNumはダイスの面の数
            DiceNum = CommandContext.Substring(CommandContext.IndexOf("n") + 1, CommandContext.IndexOf("c") - CommandContext.IndexOf("n") - 1);
            //ClearNumは成功値
            ClearNum = CommandContext.Substring(CommandContext.IndexOf("c") + 1, CommandContext.IndexOf("t") - CommandContext.IndexOf("c") - 1);
            //ClearTimesは成功度
            ClearTimes = CommandContext.Substring(CommandContext.IndexOf("t") + 1);

            //難易度がダイス面より大きい場合
            if (!(int.Parse(ClearNum) < int.Parse(DiceNum) + 1))
              return;
            //成功地がダイスを振る回数より大きいとき
            if (!(int.Parse(ClearTimes) < int.Parse(NumTimes) + 1))
              return;

            string m0 = "";
            //await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "回ふります。" + "成功値" + ClearNum + "の成功度" + ClearTimes + "です。");
            m0 += $"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "回ふります。" + "成功値" + ClearNum + "の成功度" + ClearTimes + "です。";
            int j = 0;
            seed = Environment.TickCount;
            for (int i = 0; i < int.Parse(NumTimes); i++)
            {
              Random rnd = new Random(seed++);
              string m1 = rnd.Next(1, int.Parse(DiceNum) + 1).ToString();
              if (int.Parse(m1) >= int.Parse(ClearNum))
              {
                j++;
                //await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + j.ToString() + "回目の成功：" + m1);
                m0 += $"<@{id}>" + "　:　" + j.ToString() + "回目の成功：" + m1 + "\n";
                if (j == int.Parse(ClearTimes))
                {
                  //await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "成功度" + j.ToString() + "をクリアしたのでダイスを止めます。");
                  m0 += $"<@{id}>" + "　:　" + "成功度" + j.ToString() + "をクリアしたのでダイスを止めます。" + "\n";
                  //j成功値に達するとfor文から脱出
                  if (j == int.Parse(ClearTimes))
                    break;
                }
              }
              else
              {
                m0 += $"<@{id}>" + "　:　" + "失敗：" + m1 + "\n";
                //await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "失敗：" + m1);
              }
            }
            await message.Channel.SendMessageAsync(m0);
          }
        }
      }

      ////////////////////////////////////////////////////////////////////////////
      if (CommandContext.Substring(0, 2) == "!r")
      {
        if (!(CommandContext.Substring(2) == ""))
        {
          NumTimes = CommandContext.Substring(2, CommandContext.IndexOf("d") - 2);
          DiceNum = CommandContext.Substring(CommandContext.IndexOf("d") + 1);
          await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "回振ります");
          List<int> ms = new List<int>();
          for (int i = 0; i < int.Parse(NumTimes); i++)
          {
            ms.Add(Random_basic(DiceNum));
          }
          var s = String.Join(",", ms);
          await message.Channel.SendMessageAsync($"<@{id}>" + "　:　出目は" + s + "で、それらの和は" + List_Sum(ms) + "です");
        }
      }
    }

    private Task Log(LogMessage message)
    {
      Console.WriteLine(message.ToString());
      return Task.CompletedTask;
    }
  }
}