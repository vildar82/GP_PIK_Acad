﻿using System;
using System.Collections.Generic;
using PIK_GP_Acad.Elements.Blocks;
using PIK_GP_Acad.Elements.Blocks.BlockSection;

namespace PIK_GP_Acad.BlockSection
{
   public class AlphanumComparatorFast : IComparer<string>
   {
      public int Compare(string s1, string s2)
      {
         if (s1 == null)
         {
            return 0;
         }
         if (s2 == null)
         {
            return 0;
         }

         int len1 = s1.Length;
         int len2 = s2.Length;
         int marker1 = 0;
         int marker2 = 0;

         // Walk through two the strings with two markers.
         while (marker1 < len1 && marker2 < len2)
         {
            char ch1 = s1[marker1];
            char ch2 = s2[marker2];

            // Some buffers we can build up characters in for each chunk.
            char[] space1 = new char[len1];
            int loc1 = 0;
            char[] space2 = new char[len2];
            int loc2 = 0;

            // Walk through all following characters that are digits or
            // characters in BOTH strings starting at the appropriate marker.
            // Collect char arrays.
            do
            {
               space1[loc1++] = ch1;
               marker1++;

               if (marker1 < len1)
               {
                  ch1 = s1[marker1];
               }
               else
               {
                  break;
               }
            } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

            do
            {
               space2[loc2++] = ch2;
               marker2++;

               if (marker2 < len2)
               {
                  ch2 = s2[marker2];
               }
               else
               {
                  break;
               }
            } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

            // If we have collected numbers, compare them numerically.
            // Otherwise, if we have strings, compare them alphabetically.
            string str1 = new string(space1);
            string str2 = new string(space2);

            int result;

            if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
            {
               int thisNumericChunk = int.Parse(str1);
               int thatNumericChunk = int.Parse(str2);
               result = thisNumericChunk.CompareTo(thatNumericChunk);
            }
            else
            {
               result = str1.CompareTo(str2);
            }

            if (result != 0)
            {
               return result;
            }
         }
         return len1 - len2;
      }
   }

   // Тип секции - по Наименованию и по кол этажей
   public class SectionType : IComparable<SectionType>
   {
      private AlphanumComparatorFast comparer = new AlphanumComparatorFast();

      public SectionType(string name, int numberFloor)
      {
         Name = name;
         NumberFloor = numberFloor;
         Sections = new List<BlockSectionGP>();
      }

      public double AreaApartTotal { get; private set; }
      public double AreaBKFN { get; private set; }
      public int Count { get; private set; }
      public string Name { get; private set; }
      public int NumberFloor { get; private set; }
      public List<BlockSectionGP> Sections { get; private set; }

      public void AddSection(BlockSectionGP section)
      {
         // ? проверять соответствие добавляемой секции этому типу секций - Имя, кол этажей
         AreaApartTotal += section.AreaApartTotal;
         AreaBKFN += section.AreaBKFN;
         Count++;
         Sections.Add(section);
      }

      public int CompareTo(SectionType other)
      {
         return comparer.Compare(Name, other.Name);
      }
   }
}