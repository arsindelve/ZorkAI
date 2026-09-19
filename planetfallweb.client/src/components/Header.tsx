import AccessTimeIcon from '@mui/icons-material/AccessTime';
import {GameHeader} from '@zork-ai/shared-types';

export default function Header({
    locationName,
    time,
    score,
}: {
    locationName: string;
    time: string;
    score: string;
}) {
    return (
        <GameHeader
            locationName={locationName}
            score={score}
            primaryStat={{
                label: 'Time',
                value: time,
                icon: <AccessTimeIcon fontSize="small" />,
                testId: 'header-time',
            }}
        />
    );
}
