import { parentalRelationship, spouse } from "./members";

// Response Content Bodies
export type BaseTree = {
  id: number;
  name: string;
  member_self_id: number | null;
};

export type BaseTrees = {
  trees: Array<BaseTree>;
};

export type BaseMember = {
    id: number;
    fname: string;
    mnames: string;
    lname: string;
    sex: boolean;
    dob: Date;
    dod: Date;
    spouses: Array<spouse>;
    children: Array<parentalRelationship>;
};

export type Tree = {
    id: number;
    name: string;
    member_self_id: number | null;
    members: Array<BaseMember>;
}