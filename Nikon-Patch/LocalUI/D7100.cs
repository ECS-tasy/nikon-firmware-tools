using System;

namespace Nikon_Patch
{
    class D7100_0101 : Firmware
    {
        public D7100_0101()
        {
            p = new Package();
            Model = "D7100";
            Version = "1.01";
        }
    }

    class D7100_0102 : Firmware
    {
        public D7100_0102()
        {
            p = new Package();
            Model = "D7100";
            Version = "1.02";
        }
    }

    class D7100_0103 : Firmware
    {
        public D7100_0103()
        {
            p = new Package();
            Model = "D7100";
            Version = "1.03";
        }
    }

    class D7100_0105 : Firmware
    {
        // 24Mbps: 00 36 6E 01
        // 20Mbps: 00 2D 31 01
        // 12Mbps: 00 1B B7 00
        // 10Mbps: 80 96 98 00
        static byte[] v_orig = { 0x00, 0x36, 0x6E, 0x01, 0x00, 0x2D, 0x31, 0x01, 0x00, 0x1B, 0xB7, 0x00, 0x80, 0x96, 0x98, 0x00 };

        // 36Mbps / 36Mbps
        static byte[] v_36 = { 0x00, 0x51, 0x25, 0x02, 0x00, 0x51, 0x25, 0x02 }; 
        // 54Mbps / 54Mbps
        static byte[] v_54 = { 0x80, 0xF9, 0x37, 0x03, 0x80, 0xF9, 0x37, 0x03 }; 
        // 64Mbps / 60Mbps
        static byte[] v_64 = { 0x00, 0x90, 0xD0, 0x03, 0x00, 0x87, 0x93, 0x03 }; 

        // Targets for NQ replacement (orig NQ is v_orig[8..16])
        static byte[] v_24_20 = { 0x00, 0x36, 0x6E, 0x01, 0x00, 0x2D, 0x31, 0x01 }; // 24M / 20M

        // Offsets (Block 1 / b760105.bin)
        // 0x151C2F8 - Start
        // 7 Entries: 1080 60i, 50i, 30p, 25p, 24p, 720 60p, 50p
        static int[] offsets = { 
            0x151C2F8, // 1
            0x151C308, // 2
            0x151C318, // 3
            0x151C328, // 4
            0x151C338, // 5
            0x151C348, // 6
            0x151C358  // 7
        };

        // Time limits
        // 20 min: 80 4F 12 00 (1,200,000 ms)
        // 29m59s: 58 73 1B 00 (1,799,000 ms)
        // 3m:     20 BF 02 00 (180,000 ms)
        // 15m:    A0 BB 0D 00 (900,000 ms)

        // Replacements
        // Max (27h): 80 27 CB 05
        static byte[] v_max = { 0x80, 0x27, 0xCB, 0x05 };
        // 6 Hours: 00 97 49 01
        static byte[] v_6h = { 0x00, 0x97, 0x49, 0x01 };

        public D7100_0105()
        {
            p = new Package();
            Model = "D7100";
            Version = "1.05";

            // Define Patch Sets
            var patch_36 = new System.Collections.Generic.List<Patch>();
            var patch_54 = new System.Collections.Generic.List<Patch>();
            var patch_36_nq = new System.Collections.Generic.List<Patch>();
            var patch_54_nq = new System.Collections.Generic.List<Patch>();
            var patch_64_nq = new System.Collections.Generic.List<Patch>();

            foreach(int o in offsets)
            {
                // HQ -> 36M, NQ Unchanged
                // 16 bytes orig. Replace first 8 bytes.
                byte[] orig_8 = new byte[8];
                Array.Copy(v_orig, 0, orig_8, 0, 8);
                patch_36.Add(new Patch(1, o, orig_8, v_36));

                // HQ -> 54M
                patch_54.Add(new Patch(1, o, orig_8, v_54));

                // HQ -> 36M, NQ -> Old HQ (24/20)
                byte[] orig_nq_8 = new byte[8];
                Array.Copy(v_orig, 8, orig_nq_8, 0, 8);
                
                // 36 NQ
                patch_36_nq.Add(new Patch(1, o, orig_8, v_36));
                patch_36_nq.Add(new Patch(1, o + 8, orig_nq_8, v_24_20));

                // 54 NQ
                patch_54_nq.Add(new Patch(1, o, orig_8, v_54));
                patch_54_nq.Add(new Patch(1, o + 8, orig_nq_8, v_24_20));
                
                // 64 NQ
                patch_64_nq.Add(new Patch(1, o, orig_8, v_64));
                patch_64_nq.Add(new Patch(1, o + 8, orig_nq_8, v_24_20));
            }

            Patch[] p_36 = patch_36.ToArray();
            Patch[] p_54 = patch_54.ToArray();
            Patch[] p_36_nq = patch_36_nq.ToArray();
            Patch[] p_54_nq = patch_54_nq.ToArray();
            Patch[] p_64_nq = patch_64_nq.ToArray();

            Patches.Add(new PatchSet(PatchLevel.Beta, "Video 1080 HQ 36mbps Bit-rate", p_36, p_54, p_36_nq, p_54_nq, p_64_nq));
            Patches.Add(new PatchSet(PatchLevel.Beta, "Video 1080 HQ 54mbps Bit-rate", p_54, p_36, p_36_nq, p_54_nq, p_64_nq));
            Patches.Add(new PatchSet(PatchLevel.Beta, "Video 1080 HQ 36mbps Bit-rate NQ old HQ", p_36_nq, p_36, p_54, p_54_nq, p_64_nq));
            Patches.Add(new PatchSet(PatchLevel.Beta, "Video 1080 HQ 54mbps Bit-rate NQ old HQ", p_54_nq, p_36, p_54, p_36_nq, p_64_nq));
            Patches.Add(new PatchSet(PatchLevel.Beta, "Video 1080 HQ 64mbps Bit-rate NQ old HQ", p_64_nq, p_36, p_54, p_36_nq, p_54_nq));

            // Time Limit Patches (Reverse Engineered from D7100_0105.bin)
            Patch[] patch_video_limit = {
                new Patch(1, 0x196E90, new byte[] { 0x58, 0x73, 0x1B, 0x00 }, v_max), // 29m59s -> Max
                new Patch(1, 0x196E94, new byte[] { 0x80, 0x4F, 0x12, 0x00 }, v_max)  // 20m -> Max
            };
            Patches.Add(new PatchSet(PatchLevel.Beta, "Remove Time Based Video Restrictions", patch_video_limit));

            Patch[] patch_liveview_6h = {
                new Patch(1, 0x196E98, new byte[] { 0x20, 0xBF, 0x02, 0x00 }, v_6h)   // 3m -> 6h
            };
            Patches.Add(new PatchSet(PatchLevel.Beta, "Liveview - 3 mins to 6 hours", patch_liveview_6h));

            Patch[] patch_liveview_no_off = {
                new Patch(1, 0xAC24,   new byte[] { 0xA0, 0xBB, 0x0D, 0x00 }, v_max), // 15m -> Max (Loc 1)
                new Patch(1, 0xEBC0,   new byte[] { 0xA0, 0xBB, 0x0D, 0x00 }, v_max), // 15m -> Max (Loc 2)
                new Patch(1, 0x308668, new byte[] { 0xA0, 0xBB, 0x0D, 0x00 }, v_max)  // 15m -> Max (Loc 3)
            };
            Patches.Add(new PatchSet(PatchLevel.Beta, "Liveview No Display Auto Off", patch_liveview_no_off));

            /* Not Implemented / Need Specific Code Analysis
            Liveview Manual Control ISO/Shutter
            Clean HDMI & LCD Liveview
            NEF Compression Off
            NEF Compression Lossless
            Disable Nikon Star Eater
            BETA - True Dark Current
            BETA - True Dark Current - Menu based
            BETA - HDMI Output 1080i FullScreen Fixed
            BETA - HDMI Output 720p FullScreen
            ALPHA - True Dark Current - Menu based
            Jpeg Compression - Quality (vs. Space)
            Non-Brand Battery
            */

        }
    }
}
