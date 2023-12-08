﻿//
// PROPRIETARY AND CONFIDENTIAL
// Brain Simulator 3 v.1.0
// © 2022 FutureAI, Inc., all rights reserved
//


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using BrainSimulator.Modules;
using System.Collections.Concurrent;

namespace BrainSimulator
{
    //a thing is anything, physical object, attribute, word, action, etc.
    public class Thing
    {
        private List<Relationship> relationships = new List<Relationship>(); //synapses to "has", "is", others
        private List<Relationship> relationshipsFrom = new List<Relationship>(); //synapses from
        public IList<Relationship> RelationshipsNoCount { get { lock (relationships) { return new List<Relationship>(relationships.AsReadOnly()); } } }
        public List<Relationship> RelationshipsWriteable { get => relationships; }
        public IList<Relationship> RelationshipsFrom { get { lock (relationshipsFrom) { return new List<Relationship>(relationshipsFrom.AsReadOnly()); } } }
        public List<Relationship> RelationshipsFromWriteable { get => relationshipsFrom; }

        private string label = ""; //this is just for convenience in debugging 
        object value;
        public int useCount = 0;
        public long lastFired = 0;
        public DateTime lastFiredTime = new();

        public object V
        {
            get => value;
            set
            {
                if (value is Thing t)
                    throw new ArgumentException("Cannot add a Thing to a Thing's Value");
                this.value = value;
            }
        }

        static ConcurrentDictionary<string, Thing> labelList = new ConcurrentDictionary<string, Thing>();
        public static Thing GetThing(string label)
        {
            Thing retVal = null;
            if (labelList.TryGetValue(label.ToLower(), out retVal)) { }
            return retVal;
        }
        public static string AddThingLabel(string newLabel,Thing t)
        {
            //sets a label and appends/increments trailing digits in the event of collisions
            if (newLabel == "") return newLabel; //don't index empty lables
            labelList.TryRemove(t.label.ToLower(), out Thing dummy);
            int curDigits = -1;
            string baseString = newLabel;
            //This code allows you to put a * at the end of a label and it will auto-increment
            if (newLabel.EndsWith("*"))
            {
                curDigits = 0;
                baseString = newLabel.Substring(0, newLabel.Length - 1);
                newLabel = baseString + curDigits;
            }

            //autoincrement in the event of name collisions
            while (!labelList.TryAdd(newLabel.ToLower(), t))
            {
                curDigits++;
                newLabel = baseString + curDigits;
            }
            return newLabel;
        }
        public static void ClearLabelList()
        {
            labelList.Clear();
            hasChildType = null;
        }
        public static List<Thing> AllThingsInLabelList()
        {
            List<Thing> retVal = new();
            foreach (Thing thing in labelList.Values) { retVal.Add(thing); }
            return retVal;
        }


        public override string ToString()
        {
            string retVal = label + ": " + useCount;
            if (Relationships.Count > 0)
            {
                retVal += " {";
                foreach (Relationship l in Relationships)
                    retVal += l.T?.label + ",";
                retVal += "}";
            }
            return retVal;
        }


        //This hack is needed because add-parent/add-child rely on knowledge of the has-child relationship which may not exist yet
        static Thing hasChildType;
        static Thing HasChild
        {
            get
            {
                if (hasChildType == null)
                {
                    hasChildType = GetThing("has-child");
                    if (hasChildType == null)
                    {
                        Thing thingRoot = GetThing("Thing");
                        if (thingRoot == null) return null;
                        Thing relTypeRoot = GetThing("RelationshipType");
                        if (relTypeRoot == null)
                        {
                            hasChildType = new Thing() { Label = "has-child" };
                            relTypeRoot = new Thing() { Label = "RelationshipType" };
                            thingRoot.AddRelationship(relTypeRoot, hasChildType);
                            relTypeRoot.AddRelationship(hasChildType, hasChildType);
                        }
                    }
                }
                return hasChildType;
            }
        }

        public string Label
        {
            get => label;
            set
            {
                if (value == label) return; //label is unchanged
                label = AddThingLabel(value,this);
            }
        }

        private  IList<Thing> RelationshipsOfType(Thing relType, bool useRelationshipFrom=false)
        {
            IList<Thing> retVal= new List<Thing>();
            if (!useRelationshipFrom)
            {
                lock (relationshipsFrom)
                {
                    foreach (Relationship r in relationships)
                        if (r.relType != null && r.relType == relType && r.source == this)
                            retVal.Add(r.target);
                }
            }
            else
            {
                lock (relationshipsFrom)
                {
                    foreach (Relationship r in relationshipsFrom)
                        if (r.relType != null && r.relType == relType && r.target == this)
                            retVal.Add(r.source);
                }
            }
            return retVal;
        }
        private bool IsKindOf(Thing thingType)
        {
            if (this == thingType) return true;
            foreach (Thing t in this.Parents)
                if (t.IsKindOf(thingType)) return true;
            return false;
        }


        public IList<Thing> Parents{ get => RelationshipsOfType(GetThing("has-Child"), true); }

        public IList<Thing> Children{ get => RelationshipsOfType(GetThing("has-Child"), false); }

        public IList<Relationship> Relationships
        {
            get
            {
                lock (relationships)
                {
                    foreach (Relationship r in relationships)
                        r.misses++;
                    return new List<Relationship>(relationships.AsReadOnly());
                }
            }
        }

        public IList<Relationship> RelationshipsWithoutChildren
        {
            get
            {
                List<Relationship> retVal = new();
                foreach (Relationship r in Relationships)
                    if (r.reltype == null || Relationship.TrimDigits(r.relType.Label) != "has-child") retVal.Add(r);
                return retVal;
            }
        }


        /// ////////////////////////////////////////////////////////////////////////////
        //Handle the descendents of a Thing
        //////////////////////////////////////////////////////////////
        public int GetDescendentsCount()
        {
            return DescendentsList().Count;
        }
        public IList<Thing> DescendentsList(List<Thing> descendents = null)
        {
            if (descendents == null)
            {
                descendents = new List<Thing>();
            }
            if (descendents.Count < 5000)
            {
                foreach (Thing t2 in this.Children)
                {
                    if (t2 == null) continue;
                    if (!descendents.Contains(t2))
                    {
                        descendents.Add(t2);
                        t2.DescendentsList(descendents);
                    }
                }
            }
            return descendents;
        }

        //recursively gets all descendents
        public IEnumerable<Thing> Descendents
        {
            get
            {
                IList<Thing> descendents = DescendentsList();
                for (int i = 0; i < descendents.Count; i++)
                {
                    Thing child = descendents[i];
                    yield return child;
                }
            }
        }

        //Get the ancestors of a thing with recursion

        public IList<Thing> AncestorList(List<Thing> ancestors = null, int depth = 0)
        {
            depth++;
            if (depth > 10)
                return ancestors;
            if (ancestors == null)
            {
                ancestors = new List<Thing>();
            }
            foreach (Thing t2 in this.Parents)
            {
                if (!ancestors.Contains(t2))// && ancestors.Count < 100)
                {
                    ancestors.Add(t2);
                    t2.AncestorList(ancestors, depth);
                }
                else
                { }  //track circular reference?
            }
            return ancestors;
        }

        float Exclusive(Relationship r1, Relationship r2)
        {
            //todo extend to handle instances of targets
            if (r1.target == r2.target)
            {
                var commonParents = ModuleUKS.FindCommonParents(r1.reltype, r2.reltype);
                if (commonParents.Count > 0)
                {
                    //IList<Thing> r1RelProps = GetProperties(r1.reltype);
                    //IList<Thing> r2RelProps = GetProperties(r2.reltype);

                }
            }
            return 1;
        }

        public IEnumerable<Thing> Ancestors
        {
            get
            {
                IList<Thing> ancestors = AncestorList();
                for (int i = 0; i < ancestors.Count; i++)
                {
                    Thing child = ancestors[i];
                    yield return child;
                }
            }
        }

        public bool HasAncestorLabeled(string label)
        {
            IList<Thing> ancestors = AncestorList();
            for (int i = 0; i < ancestors.Count; i++)
            {
                Thing parent = ancestors[i];
                if (parent.label == label)
                    return true;
            }
            return false;
        }
        public bool HasAncestor(Thing t)
        {
            IList<Thing> ancestors = AncestorList();
            for (int i = 0; i < ancestors.Count; i++)
            {
                Thing parent = ancestors[i];
                if (parent == t)
                    return true;
            }
            return false;
        }


        public void SetFired(Thing t = null)
        {
            if (t != null)
            {
                // newParent.lastFired = MainWindow.Generation;
                t.lastFiredTime = DateTime.Now;
                t.useCount++;
            }
            else
            {
                // lastFired = MainWindow.Generation;
                lastFiredTime = DateTime.Now;
                useCount++;
            }
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// </summary>

        //RELATIONSHIPS

        //add a relationship from this thing to the specified thing
        public Relationship AddRelationship(Thing t, float weight = 1)
        {
            if (t == null) return null; //do not add null relationship or duplicates

            Relationship r = relationships.Find(l => l.source == this && l.T == t);
            if (r != null)
            {
                r.weight = weight;
                return r;
            }
            Relationship newLink;
            newLink = new Relationship { source = this, T = t, weight = weight};
            lock (relationships)
            {
                relationships.Add(newLink);
            }
            lock (t.relationshipsFrom)
            {
                t.relationshipsFrom.Add(newLink);
            }
            return newLink;
        }


        public void RemoveRelationship(Thing t)
        {
            if (t == null) return;

            foreach (Relationship r in Relationships)
            {
                if (r.relType is not null && r.reltype.Label != "has-child") //hack for performance
                {
                    lock (r.relType.relationshipsFrom)
                        r.relType.relationshipsFrom.RemoveAll(v => v.target == t && v.source == this);
                }
            }
            lock (relationships)
            {
                relationships.RemoveAll(v => v.target == t);
            }
            lock (t.relationshipsFrom)
            {
                t.relationshipsFrom.RemoveAll(v => v.source == this);
            }
        }

        public void RemoveRelationship(Relationship r)
        {
            if (r == null) return;
            if (r.reltype == null) return;
            if (r.source == null)
            {
                lock (r.relType.RelationshipsFromWriteable)
                {
                    lock (r.target.RelationshipsFromWriteable)
                    {
                        r.relType.RelationshipsFromWriteable.Remove(r);
                        r.target.RelationshipsFromWriteable.Remove(r);
                    }
                }
            }
            else if (r.target == null)
            {
                lock (r.source.RelationshipsWriteable)
                {
                    lock (r.relType.RelationshipsFromWriteable)
                    {
                        r.source.RelationshipsWriteable.Remove(r);
                        r.relType.RelationshipsFromWriteable.Remove(r);
                    }
                }
            }
            else
            {
                lock (r.source.RelationshipsWriteable)
                {
                    lock (r.relType.RelationshipsFromWriteable)
                    {
                        lock (r.target.RelationshipsFromWriteable)
                        {
                            r.source.RelationshipsWriteable.Remove(r);
                            r.relType.RelationshipsFromWriteable.Remove(r);
                            r.target.RelationshipsFromWriteable.Remove(r);
                        }
                    }
                }
            }
            foreach (ClauseType c in r.clauses)
                RemoveRelationship(c.clause);
        }

        public Relationship HasRelationship(Thing t)
        {
            foreach (Relationship L in Relationships)
                if (L.T == t) return L;
            return null;
        }

        public Thing HasRelationshipWithParent(Thing t)
        {
            foreach (Relationship L in Relationships)
                if (L.T.Parents.Contains(t)) return L.T;
            return null;
        }

        public Thing HasRelationshipWithAncestorLabeled(string s)
        {
            foreach (Relationship L in Relationships)
            {
                if (L.T != null)
                {
                    Thing t = L.T.AncestorList().FindFirst(x => x.Label == s);
                    if (t != null) return L.T;
                }
            }
            return null;
        }

        //(send a negative value to decrease a relationship weight)
        public float AdjustRelationship(Thing t, float incr = 1)
        {
            //change any exisiting link or add a new one
            Relationship existingLink = Relationships.FindFirst(v => v.T == t);
            if (existingLink == null && incr > 0)
            {
                existingLink = AddRelationship(t, incr);
            }
            if (existingLink != null)
            {
                if (existingLink.relType is not null)
                {
                    //TODO adjust the weight of relationshipType revers link
                }
                else
                {
                    Relationship reverseLink = existingLink.T.relationshipsFrom.Find(v => v.T == this);
                    existingLink.weight += incr;
                    if (incr > 0) existingLink.hits++;
                    if (incr < 0) existingLink.misses++;
                    reverseLink.weight = existingLink.weight;
                    reverseLink.hits = existingLink.hits;
                    reverseLink.misses = existingLink.misses;
                }
                if (existingLink.weight < 0)
                {
                    return -1;
                }
                return existingLink.weight;
            }
            return 0;
        }

        public Relationship AddRelationship(Thing t2, Thing relationshipType)
        {
            if (relationshipType == null)
                return null;

            relationshipType.SetFired();
            Relationship r = HasRelationship(t2, relationshipType);
            if (r != null)
            {
                AdjustRelationship(r.T);
                return r;
            }
            r = new Relationship()
            {
                relType = relationshipType,
                source = this,
                T = t2,
            };
            if (t2 != null)
            {
                lock (relationships)
                    lock (t2.relationshipsFrom)
                        lock (relationshipType.relationshipsFrom)
                        {
                            RelationshipsWriteable.Add(r);
                            t2.RelationshipsFromWriteable.Add(r);
                            relationshipType.RelationshipsFromWriteable.Add(r);
                        }
            }
            else
            {
                lock (relationships)
                    lock (relationshipType.relationshipsFrom)
                    {
                        RelationshipsWriteable.Add(r);
                        relationshipType.RelationshipsFromWriteable.Add(r);
                    }
            }
            return r;
        }

        public Relationship HasRelationship(Thing t2, Thing relationshipType)
        {
            Relationship retVal = null;
            foreach (Relationship r in Relationships)
            {
                if ((r.relType == relationshipType || relationshipType == null) && r.target == t2)
                {
                    retVal = r;
                    break;
                }
            }
            return retVal;
        }


        public void RemoveRelationship(Thing t2, Thing relationshipType)
        {
            RemoveRelationship(t2);
        }



        //returns all the matching refrences
        public List<Relationship> GetRelationshipsWithAncestor(Thing t)
        {
            List<Relationship> retVal = new List<Relationship>();
            lock (relationships)
            {
                for (int i = 0; i < Relationships.Count; i++)
                {
                    if (Relationships[i].T.HasAncestor(t))
                    {
                        retVal.Add(Relationships[i]);
                    }
                }
                return retVal.OrderBy(x => -x.Value).ToList();
            }
        }

        public List<Relationship> GetRelationshipByWithAncestor(Thing t)
        {
            List<Relationship> retVal = new List<Relationship>();
            for (int i = 0; i < relationshipsFrom.Count; i++)
            {
                if (relationshipsFrom[i].source.HasAncestor(t))
                {
                    retVal.Add(relationshipsFrom[i]);
                }
            }
            return retVal.OrderBy(x => -x.Value).ToList();
        }

        public void AddParent(Thing newParent)//, SentenceType sentencetype = null)
        {
            if (newParent == null) return;
            if (!Parents.Contains(newParent))
            {
                newParent.AddRelationship(this, HasChild);//, sentencetype);
            }
        }

        public void RemoveParent(Thing t)
        {
            t.RemoveRelationship(this);
        }

        public Relationship AddChild(Thing t)
        {
            return AddRelationship(t, HasChild);
        }

        public void RemoveChild(Thing t)
        {
            RemoveRelationship(t, HasChild);
        }

    }

    //this is a modification of Thing which is used to store and retrieve the KB in XML
    //it eliminates circular references by replacing Thing references with int indexed into an array and makes things much more compact
    public class SThing
    {
        public string label = ""; //this is just for convenience in debugging and should not be used
        public List<SRelationship> relationships = new();
        object value;
        public object V { get => value; set => this.value = value; }
        public int useCount;
    }
}
