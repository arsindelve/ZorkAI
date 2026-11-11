import DirectionsRunIcon from '@mui/icons-material/DirectionsRun';
import {GameHeader} from '@zork-ai/shared-types';

export default function Header({
    locationName,
    moves,
    score,
}: {
    locationName: string;
    moves: string;
    score: string;
}) {
    return (
        <GameHeader
            locationName={locationName}
            score={score}
            primaryStat={{
                label: 'Moves',
                value: moves,
                icon: <DirectionsRunIcon fontSize="small" />,
                testId: 'header-moves',
            }}
        />
    );
}
