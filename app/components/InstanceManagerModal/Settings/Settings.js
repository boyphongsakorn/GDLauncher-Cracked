import React, { useState, useEffect } from 'react';
import { connect } from 'react-redux';
import { routerActions } from 'connected-react-router';
import { bindActionCreators } from 'redux';
import { Form, Input, Icon, Button, message, Slider, Switch } from 'antd';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUndo, faCheck } from '@fortawesome/free-solid-svg-icons';
import path from 'path';
import { promisify } from 'util';
import fs, { stat } from 'fs';
import log from 'electron-log';
import Card from '../../Common/Card/Card';
import styles from './Settings.scss';
import JavaMemorySlider from './JavaMemorySlider';
import { PACKS_PATH, DEFAULT_ARGS } from '../../../constants';
import { history } from '../../../store/configureStore';
import { setJavaArgs } from '../../../actions/settings';
import ForgeManager from './ForgeManager';

const FormItem = Form.Item;

type Props = {
  setJavaArgs: () => void,
  javaArgs: string,
  overrideJavaArgs: string
};

function Instances(props: Props) {
  const { overrideJavaArgs } = props;

  const [is64bit, setIs64bit] = useState(true);
  const [instanceConfig, setInstanceConfig] = useState(null);
  const [checkingForge, setCheckingForge] = useState(true);
  const [unMounting, setUnMounting] = useState(false);
  const [overrideArgs, setOverrideArgsInput] = useState(null);
  const [switchState, setSwitchState] = useState(false);
  const [overrideJavaMemory, setOverrideJavaMemory] = useState(null);

  let watcher = null;

  const updateJavaArguments = javaArguments => {
    setOverrideArgsInput(javaArguments);
  };

  // resetArgs the global arguments to the default one
  const resetArgs = async () => {
    updateJavaArguments(DEFAULT_ARGS);
    const config = JSON.parse(
      await promisify(fs.readFile)(
        path.join(PACKS_PATH, props.instance, 'config.json')
      )
    );
    const modifiedConfig = JSON.stringify({
      ...config,
      overrideArgs: DEFAULT_ARGS
    });
    await promisify(fs.writeFile)(
      path.join(PACKS_PATH, props.instance, 'config.json'),
      modifiedConfig
    );
  };

  // Set the changed java arguments
  const updateArgs = async () => {
    if (overrideArgs) {
      const config = JSON.parse(
        await promisify(fs.readFile)(
          path.join(PACKS_PATH, props.instance, 'config.json')
        )
      );
      const modifiedConfig = JSON.stringify({ ...config, overrideArgs });
      await promisify(fs.writeFile)(
        path.join(PACKS_PATH, props.instance, 'config.json'),
        modifiedConfig
      );
      updateJavaArguments(overrideArgs);
    } else message.error('Enter Valid Arguments');
  };

  async function configManagement() {
    try {
      const configFile = JSON.parse(
        await promisify(fs.readFile)(
          path.join(PACKS_PATH, props.instance, 'config.json')
        )
      );
      if (configFile.overrideArgs) {
        setSwitchState(true);
        setOverrideJavaMemory(configFile.overrideMemory);
      } else setSwitchState(false);

      setOverrideArgsInput(configFile.overrideArgs);
      setOverrideJavaMemory(configFile.overrideMemory);
      console.log(configFile.overrideMemory);
      console.log(overrideJavaMemory);
      setInstanceConfig(configFile);

      watcher = fs.watch(
        path.join(PACKS_PATH, props.instance, 'config.json'),
        { encoding: 'utf8' },
        async (eventType, filename) => {
          const config = JSON.parse(
            await promisify(fs.readFile)(
              path.join(PACKS_PATH, props.instance, 'config.json')
            )
          );
          setInstanceConfig(config);
        }
      );
    } catch (err) {
      log.error(err.message);
    } finally {
      setCheckingForge(false);
    }
  }

  async function checkMemorySlider() {
    const config = JSON.parse(
      await promisify(fs.readFile)(
        path.join(PACKS_PATH, props.instance, 'config.json')
      )
    );
    setOverrideJavaMemory(config.overrideMemory);
  }

  useEffect(() => {
    configManagement();
    return () => {
      watcher.close();
      checkMemorySlider();
    };
  }, []);

  function handleSubmit(e) {
    e.preventDefault();
    props.form.validateFields(async (err, values) => {
      if (!err) {
        try {
          await promisify(fs.access)(path.join(PACKS_PATH, values.packName));
          message.warning('An instance with this name already exists.');
        } catch (err) {
          const packFolder = path.join(PACKS_PATH, props.instance);
          const newPackFolder = path.join(PACKS_PATH, values.packName);
          await promisify(fs.rename)(packFolder, newPackFolder);
          props.close();
        }
      }
    });
  }

  async function toggleJavaArguments(e) {
    try {
      const config = JSON.parse(
        await promisify(fs.readFile)(
          path.join(PACKS_PATH, props.instance, 'config.json')
        )
      );
      // setOverrideJavaMemory(config.overrideArgs);
      if (config.overrideArgs === undefined && e) {
        const modifiedConfig = JSON.stringify({
          ...config,
          overrideArgs: props.overrideJavaArgs
        });
        await promisify(fs.writeFile)(
          path.join(PACKS_PATH, props.instance, 'config.json'),
          modifiedConfig
        );
        setOverrideJavaMemory(config.overrideArgs);
        setOverrideArgsInput(props.overrideJavaArgs);
        setSwitchState(true);
      } else if (config.overrideArgs && !e) {
        const modifiedConfig = JSON.stringify(_.omit(config, 'overrideArgs'));
        await promisify(fs.writeFile)(
          path.join(PACKS_PATH, props.instance, 'config.json'),
          modifiedConfig
        );
        setSwitchState(false);
      }
    } catch (err) {
      console.error(err);
    }
  }

  async function updateMemory(v) {
    try {
      console.log('GESU', v);
      const config = JSON.parse(
        await promisify(fs.readFile)(
          path.join(PACKS_PATH, props.instance, 'config.json')
        )
      );
      const modifiedConfig = JSON.stringify({
        ...config,
        overrideMemory: v
      });
      await promisify(fs.writeFile)(
        path.join(PACKS_PATH, props.instance, 'config.json'),
        modifiedConfig
      );
      setOverrideJavaMemory(v);
    } catch (err) {
      console.error(err);
    }
  }

  const javaArgInput = (
    <div>
      <Input
        value={overrideArgs}
        style={{
          display: 'inline-block',
          maxWidth: '74%',
          marginRight: '10px',
          marginBottom: 10,
          marginTop: 4,
          backgroundColor: 'var(--secondary-color-1)',
          marginLeft: '1%'
        }}
        onChange={e => setOverrideArgsInput(e.target.value)}
      />
      <Button.Group
        style={{
          maxWidth: '60%',
          marginBottom: 10,
          marginTop: 4
        }}
      >
        <Button
          style={{
            maxWidth: '60%',
            marginBottom: 10,
            marginTop: 4
          }}
          onClick={() => updateArgs()}
          type="primary"
        >
          <FontAwesomeIcon icon={faCheck} />
        </Button>
        <Button
          style={{
            maxWidth: '60%',
            marginBottom: 10,
            marginTop: 4
          }}
          type="primary"
          onClick={() => resetArgs()}
        >
          <FontAwesomeIcon icon={faUndo} />
        </Button>
      </Button.Group>
    </div>
  );

  const { getFieldDecorator } = props.form;
  return (
    <div className={styles.container}>
      <div className={styles.innerContainer}>
        <div className={styles.content}>
          <h2>Edit Instance Settings</h2>
          <Form layout="inline" onSubmit={e => handleSubmit(e)}>
            <div>
              <div
                style={{
                  width: '100%',
                  display: 'flex',
                  height: '60px',
                  margin: 0,
                  overflowY: 'auto'
                }}
              >
                <FormItem>
                  {getFieldDecorator('packName', {
                    rules: [
                      {
                        required: true,
                        message: 'Please input a valid name',
                        pattern: new RegExp(
                          '^[a-zA-Z0-9_.-]+( [a-zA-Z0-9_.-]+)*$'
                        )
                      }
                    ],
                    initialValue: props.instance
                  })(
                    <Input
                      size="large"
                      style={{
                        width: 300,
                        display: 'inline-block',
                        height: 60
                      }}
                      prefix={
                        <Icon
                          type="play-circle"
                          theme="filled"
                          style={{ color: 'rgba(255,255,255,.8)' }}
                        />
                      }
                      placeholder="Instance Name"
                    />
                  )}
                </FormItem>
                <Button
                  icon="save"
                  size="large"
                  type="primary"
                  htmlType="submit"
                  style={{
                    width: 150,
                    display: 'inline-block',
                    height: 60
                  }}
                >
                  Rename
                </Button>
              </div>
            </div>
          </Form>
          <Card style={{ marginTop: 15 }} title="Forge Manager">
            {!checkingForge ? (
              <ForgeManager
                name={props.instance}
                data={instanceConfig}
                closeModal={props.close}
              />
            ) : null}
          </Card>

          <Card style={{ marginTop: 15, height: 'auto' }} title="Java Manager">
            <JavaMemorySlider
              // ram={props.settings.java.overrideMemory}
              ram={overrideJavaMemory}
              is64bit={is64bit}
              updateMemory={updateMemory}
              javaArguments={overrideArgs}
              instanceName={props.instance}
            />
            <div style={{ display: 'inline', verticalAlign: 'middle' }}>
              <div className={styles.mainText}>
                Java Arguments
                <Switch
                  className={styles.switch}
                  onChange={e => toggleJavaArguments(e)}
                  checked={switchState}
                />
              </div>
              {switchState ? javaArgInput : null}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}

function mapStateToProps(state) {
  return {
    settings: state.settings,
    javaArgs: state.settings.java.javaArgs,
    overrideJavaArgs: state.settings.java.overrideJavaArgs
    // overrideMemory: state.settings.java.overrideMemory
  };
}

const mapDispatchToProps = {
  setJavaArgs
};

export default Form.create()(
  connect(
    mapStateToProps,
    mapDispatchToProps
  )(Instances)
);
