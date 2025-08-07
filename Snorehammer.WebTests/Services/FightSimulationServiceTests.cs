using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Snorehammer.Web.FrontendModels;
using Snorehammer.Web.FrontendModels.Profiles;
using Snorehammer.Web.FrontendModels.Simulations;

namespace Snorehammer.Web.Services.Tests
{
    public class FightSimulationServiceTests
    {
        private FightSimulationService service;
        private UnitProfile attacker;
        private UnitProfile unitProfile;
        private AttackProfile weapon;
        private WeaponSimulation weaponSim;

        private FightSimulation sim;

        public virtual void Setup()
        {
            service = new FightSimulationService();
            weapon = new AttackProfile()
            {
                ArmorPenetration = 0,
                Attacks = 2,
                Damage = 1,
                Id = 1,
                Name = "bolt Rifle",
                Skill = 3,
                Strength = 4
            };
            attacker = new UnitProfile()
            {
                Id = 1,
                InvulnerableSave = 0,
                ModelCount = 10,
                Name = "Intercessors",
                MinimumSave = 3,
                Toughness = 4,
                Wounds = 2,
                Attacks = new List<AttackProfile>() {

                    weapon
                }
            };
            unitProfile = new UnitProfile()
            {
                Id = 1,
                InvulnerableSave = 0,
                ModelCount = 10,
                Name = "Intercessors",
                MinimumSave = 3,
                Toughness = 4,
                Wounds = 2
            };
            weaponSim = new WeaponSimulation(weapon, unitProfile, 1);
            sim = new FightSimulation()
            {
                Attacker = attacker,
                Defender = unitProfile,
                WeaponSimulations = new List<WeaponSimulation>()
                {
                    weaponSim
                }
            };
        }
        public virtual void TearDown()
        {
            sim = null;
            attacker = null;
            unitProfile = null;
            service = null;
            weaponSim = null;
            weapon = null;
        }
        [TestClass]
        public class FightSimulationArmorSaveTests : FightSimulationServiceTests
        {

            [SetUp]
            public override void Setup()
            {
                base.Setup();
            }
            [TearDown]
            public override void TearDown()
            {
                base.TearDown();
            }
            [Test]
            public void CalculateArmorSaveUnchanged()
            {
                //arrange
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveMinusOne()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesInvulnerable()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 4;
                sim.Defender.InvulnerableSave = 4;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                weaponSim.ArmorSave.Should().Be(4);
            }
            [Test]
            public void CalculateArmorSaveUsesCover()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 1;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeFalse();
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveCoverDoesNotReduceTo2()
            {
                //arrange
                sim.Attacker.Attacks[0].ArmorPenetration = 0;
                sim.Defender.HasCover = true;
                //act
                var res = service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeTrue();
                weaponSim.ArmorSave.Should().Be(3);
                res.Should().Be(3);
            }
            [Test]
            public void CalculateArmorSaveIgnoresCoverBecauseInvulnerable()
            {
                //arrange
                weaponSim.Weapon.ArmorPenetration = 3;
                weaponSim.Defender.InvulnerableSave = 4;
                weaponSim.Defender.HasCover = true;
                //act
                service.DetermineArmorSave(weaponSim);
                //assert
                weaponSim.CoverIgnored.Should().BeTrue();
                weaponSim.ArmorSave.Should().Be(4);
            }
        }
        [TestClass]
        public class FightSimulationWinnerTests : FightSimulationServiceTests
        {
            private List<Dice> diceList;
            private List<Dice> fnpDiceList;
            private Random random = A.Fake<Random>();
            private Random fnpRandom = A.Fake<Random>();
            [SetUp]
            public override void Setup()
            {
                base.Setup();
                diceList = new List<Dice>();
                fnpDiceList = new List<Dice>();
            }
            [TearDown]
            public override void TearDown()
            {
                base.TearDown();
                diceList = null;
                fnpDiceList = null;
            }
            [Test]
            public void WinnerMessageFNPBlocksNone()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                unitProfile.FeelNoPain = true;
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].FeelNoPainDice = fnpDiceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 20;
                sim.WeaponSimulations[0].Stats.PreFNPDamage = 20;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 20;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 20;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("0 of 20 wounds blocked by Feel No Pain.");
                res.Should().Contain("20 wounds inflicted to defender.");
                res.Should().Contain("The entire unit was destroyed");
            }
            [Test]
            public void WinnerMessageFNPBlocksOne()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                unitProfile.FeelNoPain = true;
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    fnpDiceList.Add(new Dice(unitProfile.MinimumSave, fnpRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].FeelNoPainDice = fnpDiceList;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 20;
                sim.WeaponSimulations[0].Stats.FeelNoPainMade = 1;
                sim.WeaponSimulations[0].Stats.PreFNPDamage = 20;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 19;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("20 out of 20 attacks broke through armor.");
                res.Should().Contain("1 of 20 wounds blocked by Feel No Pain.");
                res.Should().Contain("19 wounds inflicted to defender.");
            }
            [Test]
            public void WinnerMessageFNPBlocksAll()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                unitProfile.FeelNoPain = true;
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(6);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].FeelNoPainDice = fnpDiceList;
                sim.WeaponSimulations[0].Stats.PreFNPDamage = 20;
                sim.WeaponSimulations[0].Stats.FeelNoPainMade = 20;
                sim.WeaponSimulations[0].Stats.DamageNumber = 20;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 20;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("20 out of 20 attacks broke through armor.");
                res.Should().Contain("All wounds blocked by feel no pain. ");
                res.Should().NotContain("20 wounds inflicted to defender.");
                res.Should().NotContain("The entire unit was destroyed");
            }
            [Test]
            public void WinnerMessageUnitKilled()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 20;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 20;
                sim.WeaponSimulations[0].Stats.PreFNPDamage = 20;
                sim.WeaponSimulations[0].Stats.ModelsDestroyed = 10;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("20 out of 20 attacks broke through armor.");
                res.Should().Contain("20 wounds inflicted to defender.");
                res.Should().Contain("The entire unit was destroyed");
            }
            [Test]
            public void WinnerMessageHalfUnit()
            {
                //arrange
                var sequence = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    sequence.Add(2);
                    sequence.Add(3);
                }
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(sequence.ToArray());
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 10;
                sim.WeaponSimulations[0].Stats.DamageNumber = 10;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 10;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("10 out of 20 attacks broke through armor.");
                res.Should().Contain("10 wounds inflicted to defender.");
                res.Should().Contain("5 out of 10 models were destroyed");
                res.Should().NotContain("remaining");
            }
            [Test]
            public void WinnerMessageDamageButNoModelDestroyed()
            {
                //arrange

                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 1;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 1;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 1;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("1 out of 20 attacks broke through armor.");
                res.Should().Contain("1 wounds inflicted to defender.");
                res.Should().Contain("0 out of 10 models were destroyed");
                res.Should().Contain("A remaining model was inflicted 1 wounds, leaving it with 1 remaining");
            }
            [Test]
            public void WinnerMessageDamagedSingleModel()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                sim.WeaponSimulations[0].Stats.AttackNumber = 1;
                unitProfile.ModelCount = 1;
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 1;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 1;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 1;

                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("1 out of 1 attacks broke through armor.");
                res.Should().Contain("1 wounds inflicted to defender.");
                res.Should().Contain("The model has 1 wound(s) remaining");
            }
            [Test]
            public void WinnerMessageDestroyedSingleModel()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                sim.WeaponSimulations[0].Stats.AttackNumber = 20;
                unitProfile.ModelCount = 1;
                for (int i = 0; i < 20; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 20;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 20;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = 20;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("20 out of 20 attacks broke through armor.");
                res.Should().Contain("20 wounds inflicted to defender.");
                res.Should().Contain("The entire unit was destroyed");
            }
            [Test]
            public void WinnerMessage0Damage()
            {
                //arrange
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(3);
                for (int i = 0; i < 2; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                }
                sim.WeaponSimulations[0].Stats.AttackNumber = 2;
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 0;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = 0;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                res.Should().Contain("0 out of 2 attacks broke through armor.");

            }
        }
        [TestClass]
        public class FightSimulationSpilloverTests : FightSimulationServiceTests
        {
            private List<Dice> diceList;
            private List<Dice> fnpDiceList;
            private List<Dice> woundDiceList;
            private Random random = A.Fake<Random>();
            private Random fnpRandom = A.Fake<Random>();
            private Random woundRandom = A.Fake<Random>();
            [SetUp]
            public override void Setup()
            {
                base.Setup();
                diceList = new List<Dice>();
                fnpDiceList = new List<Dice>();
                woundDiceList = new List<Dice>();
            }
            [TearDown]
            public override void TearDown()
            {
                base.TearDown();
                diceList = null;
                fnpDiceList = null;
                woundDiceList = null;
            }
            [Test]
            public void SpilloverDoesNotHappen()
            {
                //arrange
                sim.WeaponSimulations[0].Weapon.Damage = 5;
                sim.WeaponSimulations[0].Stats.AttackNumber = 1;
                sim.WeaponSimulations[0].Weapon.WeaponsInUnit = 1;
                sim.Stats.AttackNumber = 1;
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                diceList.Add(new Dice(unitProfile.MinimumSave, random));
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = sim.WeaponSimulations[0].ArmorDice.Where(d => !d.Success).Count();
                sim.WeaponSimulations[0].Stats.DamageNumber = 5;
                sim.WeaponSimulations[0].Stats.WoundsInflicted = sim.WeaponSimulations[0].Stats.DamageNumber;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                weaponSim.Stats.ModelsDestroyed.Should().Be(1);
            }
            [Test]
            public void SpilloverUsesFeelNoPainRolls()
            {
                //arrange
                sim.Defender.Wounds = 3;
                sim.Attacker.Attacks[0].Damage = 3;
                sim.WeaponSimulations[0].Stats.AttackNumber = 2;
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                unitProfile.FeelNoPain = true;
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                //blocks 2 damage, lets 1 and 3 damage through, only killing a single model
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6, 6);
                for (int i = 0; i < sim.Attacker.Attacks[0].Attacks; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    fnpDiceList.Add(new Dice(unitProfile.FeelNoPainTarget, fnpRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].FeelNoPainDice = fnpDiceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 6;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = sim.WeaponSimulations[0].ArmorDice.Where(d => !d.Success).Count();
                sim.WeaponSimulations[0].Stats.FeelNoPainMade = sim.WeaponSimulations[0].FeelNoPainDice.Where(d => d.Success).Count();
                sim.WeaponSimulations[0].Stats.DamageNumber -= sim.Stats.FeelNoPainMade;
                sim.Stats.WoundsInflicted = sim.WeaponSimulations[0].Stats.DamageNumber;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                weaponSim.Stats.ModelsDestroyed.Should().Be(1);
            }
            [Test]
            public void SpilloverUsesVariableDamageRolls()
            {
                //arrange
                sim.Attacker.Attacks[0].IsVariableDamage = true;
                sim.Attacker.Attacks[0].VariableAttackDiceNumber = 1;
                sim.Attacker.Attacks[0].VariableDamageDiceSides = 6;
                sim.WeaponSimulations[0].Stats.AttackNumber = 2;
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                A.CallTo(() => woundRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1, 2);
                for (int i = 0; i < 2; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    woundDiceList.Add(new Dice(1, woundRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].WoundDice = woundDiceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 3;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = sim.WeaponSimulations[0].ArmorDice.Where(d => !d.Success).Count();
                sim.WeaponSimulations[0].Stats.WoundsInflicted = (int)sim.Stats.DamageNumber;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                weaponSim.Stats.ModelsDestroyed.Should().Be(1);
            }
            [Test]
            public void SpilloverUsesVariableDamageAndFeelNoPainRolls()
            {
                //arrange
                sim.WeaponSimulations[0].Weapon.IsVariableDamage = true;
                sim.WeaponSimulations[0].Weapon.VariableAttackDiceNumber = 1;
                sim.WeaponSimulations[0].Weapon.VariableDamageDiceSides = 6;
                sim.WeaponSimulations[0].Stats.AttackNumber = 2;
                sim.WeaponSimulations[0].Defender.Wounds = 3;
                A.CallTo(() => random.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(2);
                sim.WeaponSimulations[0].Defender.FeelNoPain = true;
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).Returns(1);
                //blocks 2 damage, lets 1 and 3 damage through, only killing a single model
                A.CallTo(() => fnpRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(6, 6);
                A.CallTo(() => woundRandom.Next(A<int>.That.IsEqualTo(1), A<int>.That.IsEqualTo(7))).ReturnsNextFromSequence(1, 6);
                for (int i = 0; i < 2; i++)
                {
                    diceList.Add(new Dice(unitProfile.MinimumSave, random));
                    woundDiceList.Add(new Dice(1, woundRandom));
                }
                for (int i = 0; i < 7; i++)
                {
                    fnpDiceList.Add(new Dice(6, fnpRandom));
                }
                sim.WeaponSimulations[0].ArmorDice = diceList;
                sim.WeaponSimulations[0].FeelNoPainDice = fnpDiceList;
                sim.WeaponSimulations[0].WoundDice = woundDiceList;
                sim.WeaponSimulations[0].Stats.DamageNumber = 7;
                sim.WeaponSimulations[0].Stats.ArmorSavesFailed = sim.WeaponSimulations[0].ArmorDice.Where(d => !d.Success).Count();
                sim.WeaponSimulations[0].Stats.FeelNoPainMade = sim.WeaponSimulations[0].FeelNoPainDice.Where(d => d.Success).Count();
                sim.WeaponSimulations[0].Stats.WoundsInflicted = sim.WeaponSimulations[0].Stats.DamageNumber;
                sim.WeaponSimulations[0].Stats.DamageNumber -= sim.WeaponSimulations[0].Stats.FeelNoPainMade;
                //act
                service.DealDamage(sim);
                var res = service.GenerateWinnerMessage(sim);
                //assert
                sim.Stats.DamageNumber.Should().Be(5);
                sim.Stats.ModelsDestroyed.Should().Be(1);
            }
        }
        [TestClass]
        public class FightSimulationWoundRollTests : FightSimulationServiceTests
        {
            [SetUp]
            public override void Setup()
            {
                base.Setup();
            }
            [TearDown]
            public override void TearDown()
            {
                base.TearDown();
            }
            [Test]
            [TestCase(4,4,4)]
            [TestCase(5,4,3)]
            [TestCase(8,4,2)]
            [TestCase(3,4,5)]
            [TestCase(2,4,6)]
            public void StrengthVsToughTests(int atkStr, int defTough, int expectedResult)
            {
                //arrange
                weaponSim.Weapon.Strength = atkStr;
                weaponSim.Defender.Toughness = defTough;
                //act
                service.DetermineModdedWoundTarget(weaponSim);
                //assert
                weaponSim.ModdedWoundTarget.Should().Be(expectedResult, $"str {atkStr} vs. tough {defTough}");
            }
            [Test]
            [TestCase(2,10,UnitType.Infantry, 2,2)]
            [TestCase(2,10,UnitType.Vehicle, 2,2)]
            [TestCase(2,10,UnitType.Monster, 2,2)]
            [TestCase(2,10,UnitType.Mounted, 2,2)]
            [TestCase(2,10,UnitType.Swarm, 2,2)]
            [TestCase(2,10,UnitType.Beast, 2,2)]
            [TestCase(6,2,UnitType.Beast, 5,2)]
            public void AntiWoundTests(int atkStr, int defTough, UnitType antiType, int antiTarget, int expectedResult)
            {
                //arrange
                switch (antiType)
                {
                    case UnitType.Infantry:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiInfantry = true;
                        weaponSim.Weapon.AntiInfantryValue = antiTarget;
                        break;
                    case UnitType.Monster:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiMonster = true;
                        weaponSim.Weapon.AntiMonsterValue = antiTarget;
                        break;
                    case UnitType.Vehicle:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiVehicle= true;
                        weaponSim.Weapon.AntiVehicleValue = antiTarget;
                        break;
                    case UnitType.Swarm:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiSwarm = true;
                        weaponSim.Weapon.AntiSwarmValue = antiTarget;
                        break;
                    case UnitType.Beast:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiBeast = true;
                        weaponSim.Weapon.AntiBeastValue = antiTarget;
                        break;
                    case UnitType.Mounted:
                        weaponSim.Defender.Type = antiType;
                        weaponSim.Weapon.AntiMounted = true;
                        weaponSim.Weapon.AntiMountedValue = antiTarget;
                        break;
                    default:
                        break;
                }
                weaponSim.Weapon.Strength = atkStr;
                weaponSim.Defender.Toughness = defTough;
                //act
                service.DetermineModdedWoundTarget(weaponSim);
                //assert
                weaponSim.ModdedWoundTarget.Should().Be(expectedResult, $"str {atkStr} vs. tough {defTough}, with anti value {antiTarget}");
            }
        }
    }
}