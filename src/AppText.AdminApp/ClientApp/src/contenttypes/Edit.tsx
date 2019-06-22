import React, { useContext } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { useModal } from 'react-modal-hook';
import { toast } from 'react-toastify';

import AppContext from '../apps/AppContext';
import { appConfig } from '../config/AppConfig';
import { useApiGet, useApi } from '../common/api';
import { ContentType } from './models';
import { IdParams } from '../common/routeParams';
import ContentTypeForm from './ContentTypeForm';
import Confirm from '../common/components/dialogs/Confirm';
import { globalValidationProperty } from '../config/constants';

interface EditProps extends RouteComponentProps<IdParams> {
}

const Edit: React.FunctionComponent<EditProps> = ({ match, history,  }) => {

  const { currentApp } = useContext(AppContext);
  const url = `${appConfig.apiBaseUrl}/${currentApp.id}/contenttypes/${match.params.id}`;
  const { data } = useApiGet<ContentType>(url);
  const updateContentType = useApi<ContentType>(url, 'PUT');
  const deleteContentType = useApi<ContentType>(url, 'DELETE');
   
  const handleSave = (contentType: ContentType): Promise<any> => {
    return updateContentType.callApi(contentType)
      .then(res => {
        if (res.ok) {
          toast.success(`Content type ${data.name} updated`);
          history.push('/contenttypes');
        }
        return res;
      });
  };

  const [showDeleteConfirmation, hideDeleteConfirmation] = useModal(() => (
      <Confirm
        visible={true}
        title="Delete content type"
        onOk={() => handleDelete(data, hideDeleteConfirmation)}
        onCancel={hideDeleteConfirmation}
      >
        Do you really want to delete the content type?
      </Confirm>
  ), [data]);

  const handleDelete = (contentType, hideDeleteConfirmation): Promise<any> => {
    return deleteContentType.callApi(contentType)
      .then(res => {
        if (res.ok) {
          toast.success(`Content type  ${contentType.name} deleted`);
          history.push('/contenttypes');
        }
        else {
          toast.error(Object.values(res.errors).join(','));
        }
        return res;
      })
      .catch(err => {
        hideDeleteConfirmation();
      })
  }

  return (  
    <>
      <h2>Edit content type</h2>
      {data &&
        <div className="row">
          <div className="col-lg-8">
            <ContentTypeForm contentType={data} onSave={handleSave} onDelete={showDeleteConfirmation} />
          </div>
          <div className="col-4">
          </div>        
        </div>
      }
    </>  
  );
};

export default Edit;