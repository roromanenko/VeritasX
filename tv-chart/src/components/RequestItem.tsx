import { DataCollectionJobDto } from "../api";



type RequestItemProps = {
    request: DataCollectionJobDto;
};


export const RequestItem = ({ request }: RequestItemProps) => {
    return (
        <div>
            <span>{request.id}</span>
            <br></br>
            <span>{request.state}</span>
            <br></br>
            <span>{request.symbol}</span>
            <br></br>
            <span>{request.fromUtc}</span>
        </div>
    )
}