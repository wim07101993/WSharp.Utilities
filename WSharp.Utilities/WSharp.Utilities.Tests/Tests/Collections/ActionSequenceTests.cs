using FluentAssertions;
using WSharp.Utilities.Collections;
using NUnit.Framework;

namespace WSharp.Utilities.Tests.Tests.Collections
{
    public class ActionSequenceTests
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void ExecuteSequence()
        {
            var number = 0;
            var eventCount = 0;
            var sequence = new ActionSequence<int>
            {
                { 0, new ActionSequenceAction<int>{ NextAction = 5, Action = () => number+= 5 } },
                { 1, new ActionSequenceAction<int>{ NextAction = 879, Action = () => number+= 879 } },
                { 45, new ActionSequenceAction<int>{ NextAction = 1, Action = () => number+= 1 } },
                { 879, new ActionSequenceAction<int>{ NextAction = 0, Action = () => number+= 1000 } },
                { 5, new ActionSequenceAction<int>{ Action = () => number+= 456 } },
            }
                .BeforeStep((sender, key, action) => eventCount++)
                .AfterStep((sender, key, action) => eventCount++)
                .BeforeStart((sender, key, action) =>
                {
                    eventCount++;
                    key.Should().Be(45, "the start key is 45");
                    number.Should().Be(0, "nothing has happened yet");
                })
                .AfterEnd((sender, key, action) => 
                {
                    eventCount++;
                    key.Should().Be(5, "it is the last key of the sequence");
                    number.Should().Be(5 + 879 + 1 + 1000 + 456, "that is the sum of all the actions");
                    eventCount.Should().Be(12, "1 event before, 1 after sequence and 2 for each step (5 steps)");
                });

            sequence.Start = 45;
            sequence.End = 5;

            sequence.Execute();
        }

        [Test]
        public void SequenceOrder()
        {
            var number = 0;
            var stepCount = 0;
            var sequence = new ActionSequence<int>
            {
                { 0, new ActionSequenceAction<int>{ NextAction = 5, Action = () => number+= 5 } },
                { 1, new ActionSequenceAction<int>{ NextAction = 879, Action = () => number+= 879 } },
                { 45, new ActionSequenceAction<int>{ NextAction = 1, Action = () => number+= 1 } },
                { 879, new ActionSequenceAction<int>{ NextAction = 0, Action = () => number+= 1000 } },
                { 5, new ActionSequenceAction<int>{ Action = () => number+= 456 } },
            }
                .AfterStep((sender, key, action) =>
                {
                    switch (stepCount)
                    {
                        case 0: // step 45
                            number.Should().Be(1, "0 + 1 = 1");
                            break;
                        case 1: // step 1
                            number.Should().Be(880, "1 + 879 = 880");
                            break;
                        case 2: // step 879
                            number.Should().Be(1880, "880 + 1000 = 1880");
                            break;
                        case 3: // step 0
                            number.Should().Be(1885, "1880 + 5 = 1885");
                            break;
                        case 4: // step 5
                            number.Should().Be(2341, "1885 + 456 = 2341");
                            break;
                    }
                    stepCount++;
                });

            sequence.Start = 45;
            sequence.End = 5;

            sequence.Execute();
        }

        [Test]
        public void GetEnumerator()
        {
            var number = 0;
            var stepCount = 0;
            var sequence = new ActionSequence<int>
            {
                { 0, new ActionSequenceAction<int>{ NextAction = 5, Action = () => number+= 5 } },
                { 1, new ActionSequenceAction<int>{ NextAction = 879, Action = () => number+= 879 } },
                { 45, new ActionSequenceAction<int>{ NextAction = 1, Action = () => number+= 1 } },
                { 879, new ActionSequenceAction<int>{ NextAction = 0, Action = () => number+= 1000 } },
                { 5, new ActionSequenceAction<int>{ NextAction = -1, Action = () => number+= 456 } },
            };

            sequence.Start = 45;
            sequence.End = 5;

            var enumerator = sequence.GetEnumerator();
            while (enumerator.MoveNext())
            {
                switch (enumerator.Current.NextAction)
                {
                    case 1: // step 45
                        //number.Should().Be(1, "0 + 1 = 1");
                        break;
                    case 879: // step 1
                        //number.Should().Be(880, "1 + 879 = 880");
                        break;
                    case 0: // step 879
                        if (number == 1880)
                            enumerator.Reset();
                        //number.Should().Be(1880, "880 + 1000 = 1880");
                        break;
                    case 5: // step 0
                        number.Should().Be(3765, "3760 + 5 = 3765");
                        break;
                    case -1: // step 5
                        number.Should().Be(4221, "3765 + 456 = 4221");
                        break;
                }
                stepCount++;
            }
        } 
    }
}
