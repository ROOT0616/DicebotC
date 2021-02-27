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
      string Inf_dice()
      {
        string tag = "error";
        int m1 = Random_basic("6");
        seed++;
        int m2 = Random_basic("6");
        if (m1 == 1)
        {
          if (m2 == 1)
          {
            tag = "event";
          }
          if (m2 == 2)
          {
            tag = "エクストリーム";
          }
          if (m2 == 3)
          {
            tag = "かわいい";
          }
          if (m2 == 4)
          {
            tag = "とんでも";
          }
          if (m2 == 5)
          {
            tag = "マニア";
          }
          if (m2 == 6)
          {
            tag = "オタク";
          }
        }
        if (m1 == 2)
        {
          if (m2 == 1)
          {
            tag = "音楽";
          }
          if (m2 == 2)
          {
            tag = "好きなタグ";
          }
          if (m2 == 3)
          {
            tag = "トレンド";
          }
          if (m2 == 4)
          {
            tag = "読書";
          }
          if (m2 == 5)
          {
            tag = "パフォーマンス";
          }
          if (m2 == 6)
          {
            tag = "美術";
          }
        }
        if (m1 == 3)
        {
          if (m2 == 1)
          {
            tag = "あらさがし";
          }
          if (m2 == 2)
          {
            tag = "おせっかい";
          }
          if (m2 == 3)
          {
            tag = "好きなタグ";
          }
          if (m2 == 4)
          {
            tag = "家事";
          }
          if (m2 == 5)
          {
            tag = "がり勉";
          }
          if (m2 == 6)
          {
            tag = "健康";
          }
        }
        if (m1 == 4)
        {
          if (m2 == 1)
          {
            tag = "アウトドア";
          }
          if (m2 == 2)
          {
            tag = "工作";
          }
          if (m2 == 3)
          {
            tag = "スポーツ";
          }
          if (m2 == 4)
          {
            tag = "同一のタグ";
          }
          if (m2 == 5)
          {
            tag = "ハイソ";
          }
          if (m2 == 6)
          {
            tag = "旅行";
          }
        }
        if (m1 == 5)
        {
          if (m2 == 1)
          {
            tag = "育成";
          }
          if (m2 == 2)
          {
            tag = "サビシガリ";
          }
          if (m2 == 3)
          {
            tag = "暇つぶし";
          }
          if (m2 == 4)
          {
            tag = "宗教";
          }
          if (m2 == 5)
          {
            tag = "同一のタグ";
          }
          if (m2 == 6)
          {
            tag = "ワビサビ";
          }
        }
        if (m1 == 6)
        {
          if (m2 == 1)
          {
            tag = "アダルト";
          }
          if (m2 == 2)
          {
            tag = "飲食";
          }
          if (m2 == 3)
          {
            tag = "ギャンブル";
          }
          if (m2 == 4)
          {
            tag = "ゴシップ";
          }
          if (m2 == 5)
          {
            tag = "ファッション";
          }
          if (m2 == 6)
          {
            tag = "happening";
          }
        }
        return tag;
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

            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "回ふります。" + "成功値" + ClearNum + "の成功度" + ClearTimes + "です。");

            int j = 0;


            for (int i = 0; i < int.Parse(NumTimes); i++)
            {
              Random rnd = new Random(seed++);
              string m1 = rnd.Next(1, int.Parse(DiceNum) + 1).ToString();
              if (int.Parse(m1) >= int.Parse(ClearNum))
              {
                j++;
                await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + j.ToString() + "回目の成功：" + m1);
                if (j == int.Parse(ClearTimes))
                {
                  await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "成功度" + j.ToString() + "をクリアしたのでダイスを止めます。");
                  //j成功値に達するとfor文から脱出
                  if (j == int.Parse(ClearTimes))
                    break;
                }
              }
              else
              {
                await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "失敗：" + m1);
              }
            }
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
      if (CommandContext.Substring(0, 2) == "!i")
      {
        if (CommandContext.Substring(3, 1) == "r")
        {
          if (CommandContext.Substring(4) == "")
          {
            NumTimes = "1";
          }
          else
          {
            NumTimes = CommandContext.Substring(4);
          }
          for (int i = 0; i < int.Parse(NumTimes); i++)
          {
            seed++;
            string tag;
            tag = Inf_dice();
            if (tag == "event")
            {
              await message.Channel.SendMessageAsync($"<@{id}>" + "　:　情報イベント発生！！");
            }
            else if (tag == "happening")
            {
              await message.Channel.SendMessageAsync($"<@{id}>" + "　:情報ハプニング発生！！");
            }
            else
            {
              await message.Channel.SendMessageAsync($"<@{id}>" + "　:　得られた情報タグは「" + tag + "」でした");
            }
          }
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