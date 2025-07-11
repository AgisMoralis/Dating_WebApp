import { User } from "./user";

export class MemberParameters {
    gender: string;
    minAge: number = 18;
    maxAge: number = 100;
    pageNumber: number = 1;
    pageSize: number = 5;

    constructor(user: User | null) {
        this.gender = user?.gender === 'female' ? 'male' : 'female';
    }
}