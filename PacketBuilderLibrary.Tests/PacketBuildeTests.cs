using System;
using System.Collections.Generic;
using System.Linq;
using Markov.PacketBuilderLibrary;
using NUnit.Framework;

namespace Markov.PacketBuilderLibrary.Tests
{
    [TestFixture]
    public class PacketBuildeTests
    {
        private const int PACKAGE_LENGTH = 6;
        private byte[] _packageHeder = { 0xCD, 0x56 };

        //Just one packet in buffer
        [Test]
        public void OneFullPackage()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);            

            byte[] body = new byte[]{1,2,3,4,5,6};

            builder.ProcessPart(BuildPackage(_packageHeder, body));

            Assert.AreEqual(1,packages.Count);
            CollectionAssert.AreEqual(body, packages[0]);
        }

        // Trash before packet
        [Test]
        public void PackageWithRubbishBefore()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);            

            byte[] body = new byte[]{1,2,3,4,5,6};

            builder.ProcessPart(BuildPackage(new byte[]{7,8,9}, _packageHeder, body));

            Assert.AreEqual(1,packages.Count);
            CollectionAssert.AreEqual(body, packages[0]);
        }

        // Two packets in buffer
        [Test]
        public void TwoPackagesInOneBuffer()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);

            byte[] body1 = new byte[] { 1, 2, 3, 4, 5, 6 };
            byte[] body2 = new byte[] { 7, 8, 9, 10, 11, 12 };

            builder.ProcessPart(BuildPackage(_packageHeder, body1,_packageHeder, body2));

            Assert.AreEqual(2, packages.Count);
            CollectionAssert.AreEqual(body1, packages[0]);
            CollectionAssert.AreEqual(body2, packages[1]);
        }

        // Packet is splitted into two buffers
        [Test]
        public void SplittedPackage()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);

            byte[] body = new byte[] { 1, 2, 3, 4, 5, 6 };

            builder.ProcessPart(BuildPackage(_packageHeder, new byte[]{1,2,3}));
            builder.ProcessPart(new byte[] { 4, 5, 6 });

            Assert.AreEqual(1, packages.Count);
            CollectionAssert.AreEqual(body, packages[0]);
        }

        // Trash between two packets
        [Test]
        public void TwoPackagesWithRubbish()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);

            byte[] body1 = new byte[] { 1, 2, 3, 4, 5, 6 };
            byte[] body2 = new byte[] { 7, 8, 9, 10, 11, 12 };

            builder.ProcessPart(BuildPackage(_packageHeder, body1));
            builder.ProcessPart(new byte[] { 1, 2, 3 });
            builder.ProcessPart(BuildPackage(_packageHeder, body2));

            Assert.AreEqual(2, packages.Count);
            CollectionAssert.AreEqual(body1, packages[0]);
            CollectionAssert.AreEqual(body2, packages[1]);
        }

        // Trash between header
        // Header splitted into two buffers
        [Test]
        public void SeparatedHeaderWithRubish()
        {
            List<byte[]> packages = new List<byte[]>();
            PacketBuilder builder = new PacketBuilder(_packageHeder, PACKAGE_LENGTH);
            builder.PackageReceived += (sender, package) => packages.Add(package);

            byte[] body = new byte[] { 1, 2, 3, 4, 5, 6 };

            builder.ProcessPart(BuildPackage(new byte[]{1,2,3}, new byte[]{_packageHeder[0]}));
            builder.ProcessPart(BuildPackage(new byte[]{_packageHeder[1]}, body));

            Assert.AreEqual(1, packages.Count);
            CollectionAssert.AreEqual(body, packages[0]);
        }


        private byte[] BuildPackage(params byte[][] parts)
        {
            byte[] buffer = new byte[parts.Sum(x => x.Length)];
            int index = 0;

            for(int i = 0; i<parts.Length; i++)
            {
                Array.Copy(parts[i], 0, buffer, index, parts[i].Length);
                index += parts[i].Length;
            }

            return buffer;
        }
    }
}
