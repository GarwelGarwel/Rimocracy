# Rimocracy
RimWorld mod to add politics to the game

## Governance

Rimocracy introduces the concept of Governance Quality (or simply Governance). It is a value, measured between 0% and 100%, that affects several important aspects of the game:

- Governance directly affects global work speed (which influences many other work-related stats) and negotiation ability. Governance of 0% slows your colonists down and reduces their negotiation powers by 25% while 100% Governance increases these values by the same percentage. It is always better to have high Governance.
- Governance also affects boosts stats that use the so-called Focus Skill, which is semi-randomly selected based on your leader's skills. For instance, Intellectual focus skill will improve research speed. The effect usually ranges from nothing for zero Governance to 50% for maximum Governance. So if you have 100% governance and Intellectual focus skill, your research speed will be increased by 125% (because of Global Work Speed bonus) x 150% (because of the Focus Skill) = 187.5%.
- However, Governance always tends to deteriorate, the faster the higher it currently is and the more citizens your nation has. For instance, if you only have three citizens and 50% of Governance, it will decay at 1.8% per day, but if you have 15 and 100% Governance, you will lose 10.9% of it every day.
- To improve Governance, your leader needs to do Govern job. It is done at a Research Station, a throne (if you own Royalty DLC), or the Management Desk from the Colony Manager mod. By default, doing this job increases Governance by 1% every hour, but it is affected by the workstation (thrones are better than research stations, for isntance), leader's Intellectual and Social stats and other factors, most importantly the impressiveness of the room. So better give them a good place to govern from.
- When your leader changes (see more about it below), your Governance is partially reset to the mid-point between its old value and 50%. So, if you had 70% Governance, your new leader will inherit only 60%.

## Leaders and Succession

As you probably understood from the text above, player nations now have their leader. Leaders who maintain good Governance enjoy well-deserved respect from their fellow citizens while those who fail at their jobs will be despised.

Leader's term can be limited (possible options are Quadrum, which is default, Half-year or Year) or indefinite, i.e. for life. When you only start playing, or when the leader's term has expired, or they died, a new leader is chosen. It is called succession.

There are several succession types that you can choose from in the settings:

- *Election*: default type, when the candidate who gets the most votes is chosen (see details below)
- *Lot*: the leader is chosen randomly from all eligible candidates
- *Seniority*: the oldest (by biological age) citizen is chosen

## Elections

If your succession type is set to election, all citizens (i.e. free colonists aged 16+) vote for their preferred candidate. Whoever gets the most votes, wins. Voters take into account their opinion of the candidates, any backstories they have in common (they prefer to vote for candidates of a similar background) and other factors. Royalty titles also make candidates more attractive to voters. A colonist can't vote for themselves.

If your nation is relatively small, every voter is also a candidate. But if you have at least 8 citizens, elections are preceded by *campaigns*.

## Election Campaigns

Three days before an election, two most popular candidates are chosen and a campaign starts. During the campaign, each candidate tries to sway other citizens (except other candidates, of course) in their favour. Sway attempts are not shown as actual interactions between pawns; they can happen between any two pawns on the same map, who are not downed or in a mental state.

The chance of a successful sway depends on the swayer's Social Impact stat. Successful sways increase political sympathy of a voter towards the candidate, which heavily influences their vote (technically, 1 point of political sympathy is similar to 25 points of opinion). However, such sympathy reduces over time. Only a maximum of 4 points of political sympathy are allowed for each voter.

A successful sway may also lead to a recruitment of the voter as a core supporter of the respective candidate. Core supporters can sway (and recruit) other voters, thus creating a snowball effect. Chance of recruitment is affected by the voter's level of support for the candidate and it usually requires several successful sways. Numbers of supporters for each candidate are shown in the Politics tab.

Candidates and their supporters usually hang together and dislike their competitors. The loser and winner in the campaign also gain respective mood modifiers.

## FAQ

**Q:** Nothing works, and the Politics tab says I don't have enough citizens.

**A:** If you've just run the mod for the first time in this save, wait a few seconds to let it start. Also note that not every colonist counts as a "citizen", but only free (i.e. not imprisoned by other factions, nor your prisoners, nor slaves from Simple Slavery) and of legal age, i.e. 16+. Colonists in cryptosleep also temporarily waive their citizen rights.

**Q:** My Goverance is falling. How do I increase it?

**A:** You need a Research Desk, a throne or a Management Desk. Preferably, put it into an impressive room. You also need a leader of your colony and he/she must have Governing enabled in Work Tab. I recommend to enable this work type for everyone: only the leader can use it anyway, so there is no problem if other colonists also have it. If your leader is too busy with other things, you can manually order them to "prioritize governing".

**Q:** How can I change the focus skill?

**A:** You can't. It is decided by the leader when he/she is chosen. The leaders tend to prefer skills they are good at. When the same leader is reelected, the focus skill can change (but is less likely to do so). If you dislike the current focus skill, your best hope is to have a better one after the next election. This is politics for you.

**Q:** Will you add <your dream feature>?

**A:** Maybe. Suggest and I'll see if I can/want to do that. I plan to work on this mod for quite some time.

## Stuff

The source code and latest release will always be available on [Github](https://github.com/GarwelGarwel/Rimocracy). Issue reports and pull requests are welcome.

This mod is distributed under MIT License.
