import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import {WelcomeDialog} from '@zork-ai/shared-types';

export default function ZorkWelcomeDialog({
    open,
    handleClose,
    handleWatchVideo,
}: {
    open: boolean;
    handleClose: () => void;
    handleWatchVideo: () => void;
}) {
    return (
        <WelcomeDialog
            open={open}
            handleClose={handleClose}
            title="Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!"
            aboutTitle="About Zork AI"
            paragraphs={[
                'This is a modern re-imagining of the iconic text adventure game Zork I. While the story remains true to the original, Zork AI introduces AI-powered parsing and dynamic responses for a richer, more immersive experience. It understands complex commands far beyond what the original could handle and even delivers witty, sarcastic replies when you try something unexpected.',
                'Zork AI is interactive fiction. You control the story by typing commands in plain English in order to explore, solve puzzles, and shape the adventure. To get started, type a command in the grey box below and press Enter/Return. The game will process your input and continue the story based on your instructions.',
            ]}
            commands={[
                'open the mailbox',
                'go south',
                'jump up and down',
                'tell me about the great underground empire',
            ]}
            commandsHeading="Need inspiration? Try:"
            closing="See if you can make your way into the house—and from there, descend into the Great Underground Empire, where the real adventure begins!"
            attribution={
                <>
                    All credit for the original story, puzzles and Zork universe goes to{' '}
                    <a
                        href="https://en.wikipedia.org/wiki/Zork"
                        target="_blank"
                        style={{color: 'inherit', textDecoration: 'underline'}}
                    >
                        Tim Anderson, Marc Blank, Bruce Daniels, and Dave Lebling - the brilliant
                        minds behind the classic game.
                    </a>
                </>
            }
            primaryAction={{
                label: 'Watch an intro video',
                icon: <PlayArrowIcon />,
                onClick: handleWatchVideo,
            }}
        />
    );
}
