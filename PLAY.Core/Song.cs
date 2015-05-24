using System;

namespace PLAY.Core
{
    public class Song : IEquatable<Song>
    {
        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string Genre { get; set; }

        public string Path { get; private set; }

        public DateTime DateAdded { get; private set; }

        // Initialises new Song class instance
        public Song(string path, DateTime dateAdded)
        {
            Path = path;
            DateAdded = dateAdded;

            Album = String.Empty;
            Artist = String.Empty;
            Genre = String.Empty;
            Title = String.Empty;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Song))
                return false;

            var other = (Song) obj;

            return Path == other.Path;
        }

        public override int GetHashCode()
        {
            return new {Path, DateAdded}.GetHashCode();
        }

        public bool Equals(Song other)
        {
            return Equals((object) other);
        }
    
    }
}
