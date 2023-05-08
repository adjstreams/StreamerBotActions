using System;

public class CPHInline
{
	public bool Execute()
	{
		var sender = args["targetUser"].ToString();
		var recipient = args["rawInput"].ToString();

		string[] gifts = { 
							"a piece of coal",
							"a fashionable handbag",
							"a game that {sender} really loves and wants everyone else to play it",
							"a steam game that {recipient} actually wanted",
							"perfume",
							"a gifted sub to ADJ",
							"instant regret chocolate",
							"a box of Beanboozled that {sender} taste-tested first to ensure it only contained the nasty ones",
							"a 2022 wall calendar",
							"a pet snake",
							"a DVD of the best film ever - Cats",
							"ADJ Merch - A bath towel Monogrammed with the letters ADJ",
							"anti-aging wrinkle cream",
							"socks with ADJ's face on them",
							"a voodoo doll that looks like ADJ"
						}; 
		string[] efforts = { "gave it a lot of thought", "bought something at the last minute", "regifted an unwanted present", "shoplifted", "robbed santa" }; 

		Random rnd = new Random();
		var giftIndex = rnd.Next(gifts.Length);
		var effortIndex = rnd.Next(efforts.Length);

		var gift = gifts[giftIndex];
		var effort = efforts[effortIndex];

			gift = gift.Replace("{sender}", sender);
			gift = gift.Replace("{recipient}", recipient);

		var message = $"@{sender} {effort} and got {recipient} {gift}. (DISCLAIMER: This is a joke, gifts are entirely fictional!)";

		CPH.SendMessage(message);
		return true;
	}
}
