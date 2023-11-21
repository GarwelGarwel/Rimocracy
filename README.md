# Rimocracy
RimWorld mod to add politics to the game

## Governance

Rimocracy introduces the concept of Governance Quality (or simply Governance). It is a value, measured between 0% and 100%, that affects several important aspects of the game:

- Governance directly affects work speed and other related stats. Governance of 0% makes your colonists work slower and reduces their negotiation powers by 25% while 100% Governance increases these values by the same percentage. It is always better to have high Governance.
- When your leader is elected/appointed, a Focus Skill is semi-randomly selected based on his/her abilities. This skill receives a bigger boost. For instance, Intellectual focus skill will improve research speed. The boost ranges from nothing for zero Governance to 50% for maximum Governance. So if you have 100% governance and Intellectual focus skill, your research speed will be increased by 125% (because of Global Work Speed bonus) x 150% (because of the Focus Skill) = 187.5%.
- However, Governance always tends to deteriorate, or decay. The rate of decay is higher for colonies with more citizens and at higher Governance level. For instance, if you only have three citizens and 50% of Governance, it will decay at 1.8% per day, but if you have 15 citizens and 100% Governance, you will lose 10.9% of it every day. In fact, it's a bit more complex than that (see below on Loyalty).
- To improve Governance, your leader needs to do Govern job. It is done at a Research Station, a throne (if you own Royalty DLC), or the Management Desk from the Colony Manager mod. The effectiveness of governing is affected by the workstation (thrones are better than research stations, for isntance), leader's Social skill and other factors, most importantly the impressiveness of the room. So better give them a good place to govern from.
- When your leader changes (see more about it below), your Governance is partially reset to the mid-point between its old value and 50%. So, if you had 70% Governance, your new leader will inherit only 60%.

## Leaders and Succession

As you probably understood from the text above, player nations now have their leader. Leaders who maintain good Governance enjoy well-deserved respect from their fellow citizens while those who fail at their jobs will be despised.

Leader's term can be limited (possible options are Quadrum, Half-year or Year) or indefinite, i.e. for life. When you only start playing, or when the leader's term has expired, or they died, a new leader is chosen. It is called succession.

There are several succession types that you can choose from in the settings:

- *Election*: when the candidate who gets the most votes is chosen (see details below)
- *Lot*: the leader is chosen randomly from all eligible candidates
- *Seniority*: the oldest (by biological age) citizen is chosen
- *Nobility*: the highest-ranking noble pawn is chosen; if several are available, the one with the oldest title is selected (requires Royalcy DLC)
- *Martial*: the pawn who killed and downed the most enemies and caused the most damage (with certain weights) is chosen

A succession type is first selected when you start the game with Rimocracy or, sometimes, after your primary ideoligion changes (with Ideology DLC). It is based on your ideoligion (again, if you have Ideology).

## Elections

If your succession type is set to election, all citizens (i.e. free colonists aged 16+) vote for their preferred candidate. Whoever gets the most votes, wins. Voters take into account their opinion of the candidates, any backstories they have in common (they prefer to vote for candidates of a similar background) and other factors. Royalty titles also make candidates more attractive to voters. A colonist can't vote for themselves.

If your nation is relatively small, every voter is also a candidate. But if you have at least 8 citizens, elections are preceded by *campaigns*.

## Election Campaigns

Three days before an election, two most popular candidates are chosen and a campaign starts. During the campaign, each candidate tries to sway other citizens (except other candidates, of course) in their favour. Sway attempts are not shown as actual interactions between pawns; they can happen between any two pawns on the same map, who are not downed or in a mental state.

The chance of a successful sway depends on the swayer's Social Impact stat. Successful sways increase political sympathy of a voter towards the candidate, which heavily influences their vote (technically, 1 point of political sympathy is similar to 25 points of opinion). However, such sympathy reduces over time. Only a maximum of 4 points of political sympathy are allowed for each voter.

A successful sway may also lead to a recruitment of the voter as a core supporter of the respective candidate. Core supporters can sway (and recruit) other voters, thus creating a snowball effect. Chance of recruitment is affected by the voter's level of support for the candidate and it usually requires several successful sways. Numbers of supporters for each candidate are shown in the Politics tab.

Candidates and their supporters usually hang together and dislike their competitors. The loser and winner in the campaign also gain respective mood modifiers.

## Decisions

Political decisions allow you to change many rules, such as succession type or term duration. Other decisions have more complex effects. E.g., Egalitarianism makes Governance decay depend on the median mood of your citizens (the higher the mood, the slower decay). Most decisions have associated Governance costs and certain requirements.

Some decisions, like changing succession type, require support of the majority of your citizens. Their opinions will depend on their personalities and interests (e.g. older pawns will prefer Seniority succession and more aggressive types Martial succession).

Many decisions, even those that don't require a vote, will please or upset your citizens. E.g., the leader and his hard-core supporters will be happy to implement Cult of Personality while the rest of your citizens will have slight mood debuffs from it.

## Actions

Some game actions such as taking, releasing or executing prisoners, banishing pawns, attacking or trading with other settlements will also have their supporters and opponents, who will react accordingly. If the action is supported by your leader, it will usually increase the Governance of the colony. If the leader opposes the action, it will either cause a Governance hit (by default) or the leader can simply veto it (if you've activated the corresponding decision).

## Loyalty

Every citizen (i.e. adult, free colonist) now has a Loyalty need. It represents his or her satisfaction with how the colony is governed. It is affected by the citizen's mood, opinion of the leader and of active decisions as well as by actions. It changes very slowly taking approximately 30 days to go from 0% to 100% (though it falls faster than it rises). However, it is immediately affected by decisions, positively or negatively. There is also a special decision to quickly increase all citizens' loyalty by distributing silver among them.

Loyal pawns may tolerate (that is, not vote against) decisions they would otherwise reject. The level of loyalty you need to have this toleration depends on how strongly they feel about a particular decision or action. You can check it in the respective tooltip. If a citizen tolerates a certain decision or action, they will also have 25% less of a loyalty hit when you take it.

Seriously disloyal citizens may start protesting. Protests are effectively certain mental breaks, but they are contagious: the more colonists are protesting (relative to the colony's population), the more likely others are to join. So better keep them loyal and happy!

Loyalty also has a passive effect: loyal citizens cause less Governance decay than disloyal ones. A completely disloyal colonist counts as three 100% loyal colonists for these calculations, and a protester as four.

## Compatibility

RimWorld 1.4 (also legacy support for RimWorld 1.3)

- **Harmony 2.0** (required)
- Colony Manager
- Individuality
- Primitive Workbenches
- Rumor Has It (Continued)
- Thrones plus
- Vanilla Traits Expanded

## FAQ

**Q:** Politics tab says I don't have enough citizens.

**A:** If you've just run the mod for the first time in this save, wait a few seconds to let it start. Also note that not every colonist counts as a "citizen", but only free (i.e. not imprisoned by other factions, nor your prisoners, nor slaves from Simple Slavery) and of legal age, i.e. 16+. Colonists in cryptosleep also temporarily waive their citizen rights.

**Q:** My Governance is falling. How do I increase it?

**A:** You need a Research Desk, a throne or a Management Desk (from Colony Manager). Preferably, put it into an impressive room. You also need a leader of your colony and he/she must have Governing enabled in Work Tab. I recommend to enable this work type for everyone: only the leader can use it anyway, so there is no problem if other colonists also have it. If your leader is too busy with other things, you can manually order them to "prioritize governing".

**Q:** How can I change the focus skill?

**A:** You can't just pick the focus skill; it is semi-randomly chosen according to leader's skills. You'll have to wait for the next succession (e.g. election)--or impeach the leader.

**Q:** Will you add <your dream feature>?

**A:** Maybe. Suggest and I'll see if I can/want to do that. I plan to work on this mod for quite some time.

## Stuff

The source code and latest release will always be available on [Github](https://github.com/GarwelGarwel/Rimocracy). Issue reports and pull requests are welcome.

This mod is distributed under MIT License. Icons and textures used were provided by [Flaticon.com](https://flaticon.com) and are distributed under their license.
