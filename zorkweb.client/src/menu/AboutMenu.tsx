import HelpIcon from '@mui/icons-material/Help';
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline';
import CodeIcon from '@mui/icons-material/Code';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import MapIcon from '@mui/icons-material/Map';
import ListAltIcon from '@mui/icons-material/ListAlt';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';
import ArticleIcon from '@mui/icons-material/Article';
import {AboutMenu as SharedAboutMenu, AboutMenuItem, DialogType} from '@zork-ai/shared-types';
import config from '../../config.json';

const items: AboutMenuItem[] = [
    {
        label: 'What is this game?',
        icon: <HelpIcon fontSize="small" />,
        trackingName: 'Welcome',
        dialog: DialogType.Welcome,
    },
    {
        label: 'Watch intro video',
        icon: <PlayCircleOutlineIcon fontSize="small" />,
        trackingName: 'Video',
        dialog: DialogType.Video,
    },
    {
        label: 'See the source code',
        icon: <CodeIcon fontSize="small" />,
        trackingName: 'Repo',
        url: 'https://github.com/arsindelve/ZorkAI',
    },
    {
        label: 'Read the 1984 Infocom Manual',
        icon: <MenuBookIcon fontSize="small" />,
        trackingName: '1984 Manual',
        url: 'https://infodoc.plover.net/manuals/zork1.pdf',
    },
    {
        label: 'Read the 1982 TRS-80 Manual',
        icon: <MenuBookIcon fontSize="small" />,
        trackingName: '1982 Manual',
        url: 'https://www.mocagh.org/infocom/zorkps-manual.pdf',
    },
    {
        label: 'Look at a Map (spoilers)',
        icon: <MapIcon fontSize="small" />,
        trackingName: 'Map',
        url: 'https://www.mocagh.org/infocom/zork-map-front.pdf',
    },
    {
        label: 'Look at a Walkthrough (major spoilers!)',
        icon: <ListAltIcon fontSize="small" />,
        trackingName: 'Walkthrough',
        url: 'https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html',
    },
    {
        label: 'Play the original Zork One',
        icon: <SportsEsportsIcon fontSize="small" />,
        trackingName: 'Play Original',
        url: 'https://iplayif.com/?story=https%3A%2F%2Feblong.com%2Finfocom%2Fgamefiles%2Fzork1-r119-s880429.z3',
    },
    {
        label: 'Wikipedia Article on Zork',
        icon: <ArticleIcon fontSize="small" />,
        trackingName: 'Wikipedia',
        url: 'https://en.wikipedia.org/wiki/Zork',
    },
];

export default function AboutMenu({latestVersion}: {latestVersion: string}) {
    return (
        <SharedAboutMenu
            latestVersion={latestVersion}
            fallbackVersion={config.version}
            items={items}
        />
    );
}
