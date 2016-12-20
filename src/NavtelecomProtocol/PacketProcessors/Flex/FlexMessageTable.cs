﻿namespace NavtelecomProtocol.PacketProcessors.Flex
{
    internal static class FlexMessageTable
    {
        public static int[] FieldSizes = {
            4,
            2,
            4,
            1,
            1,
            1,
            1,
            1,
            4,
            4,
            4,
            4,
            4,
            2,
            4,
            4,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            1,
            1,
            1,
            1,
            4,
            4,
            2,
            2,
            4,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            2,
            4,
            2,
            1,
            4,
            2,
            2,
            2,
            2,
            2,
            1,
            1,
            1,
            2,
            4,
            2,
            1,


            8,
            2,
            1,
            16,
            4,
            2,
            4,
            37,
            1,
            1,
            1,
            1,
            1,
            1,
            3,
            3,
            3,
            3,
            3,
            3,
            3,
            3,
            3,
            3,
            6,
            12,
            24,
            48,
            1,
            1,
            1,
            1,
            4,
            4,
            1,
            4,
            2,
            6,
            2,
            6,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            2,
            1,
            2,
            2,
            2,
            1
        };

        public static int GetFlexMessageSize(SessionState sessionState)
        {
            var result = 0;

            for (var i = 0; i < sessionState.FieldMask.Length; ++i)
                result += (sessionState.FieldMask[i] ? FieldSizes[i] : 0);

            return result;
        }
    }
}