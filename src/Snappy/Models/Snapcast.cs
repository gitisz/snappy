using System.Collections.Generic;

namespace Snappy.Models
{
    public class Snapcast
    {
        public int Id { get; set; }

        public string Jsonrpc { get; set; }

        public Result Result { get; set; }
    }

    public class Result
    {
        public ParentServer Server { get; set; }
    }

    public class Volume
    {
        public bool Muted { get; set; }
        public int Percent { get; set; }
    }

    public class Latency
    {
        public string Id { get; set; }
        public int latency { get; set; }
    }

    public class Config
    {
        public int Instance { get; set; }
        public int Latency { get; set; }
        public string Name { get; set; }
        public Volume Volume { get; set; }
    }

    public class Host
    {
        public string Arch { get; set; }
        public string Ip { get; set; }
        public string Mac { get; set; }
        public string Name { get; set; }
        public string Os { get; set; }
    }

    public class LastSeen
    {
        public int Sec { get; set; }
        public int Usec { get; set; }
    }

    public class Snapclient
    {
        public string Name { get; set; }
        public int ProtocolVersion { get; set; }
        public string Version { get; set; }
    }

    public class Client
    {
        public Config Config { get; set; }
        public bool Connected { get; set; }
        public Host Host { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public LastSeen LastSeen { get; set; }
        public Snapclient Snapclient { get; set; }
    }

    public class Group
    {
        public List<Client> Clients { get; set; }
        public string Id { get; set; }
        public bool Muted { get; set; }
        public string Name { get; set; }
        public string StreamId { get; set; }
        public int GroupVol { get; set; }
    }

    public class Snapserver
    {
        public int ControlProtocolVersion { get; set; }
        public string Name { get; set; }
        public int ProtocolVersion { get; set; }
        public string Version { get; set; }
    }

    public class Server
    {
        public Host Host { get; set; }
        public Snapserver Snapserver { get; set; }
    }

    public class ParentServer
    {
        public List<Group> Groups { get; set; }
        public Server Server { get; set; }
        public List<Stream> Streams { get; set; }
    }

    public class ArtData
    {
        public string Data { get; set; }
        public string Extension { get; set; }
    }

    public class Metadata
    {
        public ArtData ArtData { get; set; }
        public string ArtUrl { get; set; }
        public double Duration { get; set; }
        public string Title { get; set; }
    }

    public class Properties
    {
        public bool CanControl { get; set; }
        public bool CanGoNext { get; set; }
        public bool CanGoPrevious { get; set; }
        public bool CanPause { get; set; }
        public bool CanPlay { get; set; }
        public bool CanSeek { get; set; }
        public Metadata Metadata { get; set; }
    }

    public class Query
    {
        public string Chunk_ms { get; set; }
        public string Codec { get; set; }
        public string Name { get; set; }
        public string Sampleformat { get; set; }
    }

    public class StreamUri
    {
        public string Fragment { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public Query Query { get; set; }
        public string Raw { get; set; }
        public string Scheme { get; set; }
    }

    public class Stream
    {
        public string Id { get; set; }
        public Properties Properties { get; set; }
        public string Status { get; set; }
        public StreamUri Uri { get; set; }
    }


}