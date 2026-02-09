import {SessionHandler} from "./SessionHandler";
import mixpanel, {Dict} from 'mixpanel-browser';

let sessionId = new SessionHandler();
mixpanel.init('62af8d500a6f295a71ef335b6ff56942');
mixpanel.identify(sessionId.getClientId());

let env_check = true;

let actions = {
    identify: (id: string) => {
        if (env_check) mixpanel.identify(id);
    },
    track: (name: string, props: Dict | undefined) => {
        if (env_check) mixpanel.track(name, props);
    },
};

export let Mixpanel = actions;
