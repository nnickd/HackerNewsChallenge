export interface Story {
    id: number,
    title: string,
    link: string,
    by?: string,
    createdAt: string,
    score?: number,
    commentCount?: number
}
