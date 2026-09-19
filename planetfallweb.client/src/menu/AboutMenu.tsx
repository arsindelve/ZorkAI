import HelpIcon from '@mui/icons-material/Help';
import CodeIcon from '@mui/icons-material/Code';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import MapIcon from '@mui/icons-material/Map';
import ListAltIcon from '@mui/icons-material/ListAlt';
import ArticleIcon from '@mui/icons-material/Article';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import {AboutMenu as SharedAboutMenu, AboutMenuItem, DialogType} from '@zork-ai/shared-types';
import config from '../../config.json';

const items: AboutMenuItem[] = [
    {
        label: 'What is Planetfall.AI?',
        icon: <HelpIcon fontSize="small" />,
        trackingName: 'Welcome',
        dialog: DialogType.Welcome,
    },
    {
        label: 'See the source code',
        icon: <CodeIcon fontSize="small" />,
        trackingName: 'Repo',
        url: 'https://github.com/arsindelve/ZorkAI',
    },
    {
        label: 'Read the Original Infocom Manual',
        icon: <MenuBookIcon fontSize="small" />,
        trackingName: 'Planetfall Manual',
        url: 'https://infodoc.plover.net/manuals/planetfa.pdf',
    },
    {
        label: 'Look at a Map (spoilers)',
        icon: <MapIcon fontSize="small" />,
        trackingName: 'Map',
        url: 'https://infodoc.plover.net/maps/planetfa.pdf',
    },
    {
        label: 'Look at a Walkthrough (major spoilers)',
        icon: <ListAltIcon fontSize="small" />,
        trackingName: 'Walkthrough',
        url: 'http://www.eristic.net/games/infocom/planetfall.html',
    },
    {
        label: 'Wikipedia Article on Planetfall',
        icon: <ArticleIcon fontSize="small" />,
        trackingName: 'Wikipedia',
        url: 'https://en.wikipedia.org/wiki/Planetfall',
    },
    {
        label: 'Play the Original Infocom Version',
        icon: <PlayArrowIcon fontSize="small" />,
        trackingName: 'Playable Version',
        url: 'https://archive.org/details/a2_Planetfall_1983_Infocom_a',
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
