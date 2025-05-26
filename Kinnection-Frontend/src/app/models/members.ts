// Response Content Bodies
export type parentalRelationship = {
  id: number;
  parent_id: number;
  child_id: number;
  adopted: Date | null;
};

export type educationHistory = {
  id: number;
  started: Date;
  ended: Date;
  title: string;
  organization: string | null;
};

export type spouse = {
  id: number;
  husband_id: number;
  wife_id: number;
  started: Date | null;
  ended: Date | null;
};
