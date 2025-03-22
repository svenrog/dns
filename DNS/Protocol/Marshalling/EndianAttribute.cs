﻿using System;

namespace DNS.Protocol.Marshalling
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class EndianAttribute(Endianness endianness) : Attribute
    {
        public Endianness Endianness { get; } = endianness;
    }
}
