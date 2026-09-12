export interface PlayerDto {
    connectionId: string
    nickname: string
    countryId: string | null
    countryName: string | null
    countryLeaderIconId: string | null
    isReady: boolean
}