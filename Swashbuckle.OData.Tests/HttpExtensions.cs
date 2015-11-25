using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swashbuckle.OData.Tests
{
    public static class HttpExtensions
    {
        #region GET Support

        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.The task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        public static async Task<HttpResponseMessage> GetAsync(this HttpClient client)
        {
            return await client.GetAsync("");
        }

        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type returned by the GET request.</typeparam>
        /// <param name="client">The client used to make the request.</param>
        public static async Task<T> GetAsync<T>(this HttpClient client)
        {
            var response = await client.GetAsync("");

            await response.ValidateSuccessAsync();

            return await response.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type returned by the GET request.</typeparam>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param><exception cref="T:System.ArgumentNullException">The <paramref name="requestUri"/> was null.</exception>
        /// <returns>The content deserialized as type T</returns>
        public static async Task<T> GetAsync<T>(this HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);

            await response.ValidateSuccessAsync();

            return await response.Content.ReadAsAsync<T>();
        }

        #endregion

        #region PATCH Support

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized as JSON.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            return client.PatchAsJsonAsync(requestUri, value, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized as JSON. Includes a cancellation
        /// token to cancel the request.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of
        /// cancellation.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
        {
            return client.PatchAsync(requestUri, value, new JsonMediaTypeFormatter(), cancellationToken);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized as XML.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsXmlAsync<T>(this HttpClient client, string requestUri, T value)
        {
            return client.PatchAsXmlAsync(requestUri, value, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized as XML. Includes a cancellation
        /// token to cancel the request.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of
        /// cancellation.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsXmlAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
        {
            return client.PatchAsync(requestUri, value, new XmlMediaTypeFormatter(), cancellationToken);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized using the given formatter.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="formatter">The formatter used to serialize the value.</param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter)
        {
            return client.PatchAsync(requestUri, value, formatter, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized using the given formatter.
        /// Includes a cancellation token to cancel the request.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="formatter">The formatter used to serialize the value.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of
        /// cancellation.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
        {
            var cancellationToken1 = cancellationToken;
            return client.PatchAsync(requestUri, value, formatter, (MediaTypeHeaderValue)null, cancellationToken1);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized using the given formatter and
        /// media type String.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="formatter">The formatter used to serialize the value.</param>
        /// <param name="mediaType">
        /// The authoritative value of the Content-Type header. Can be null, in which case the  default
        /// content type of the formatter will be used.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType)
        {
            return client.PatchAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized using the given formatter and
        /// media type String. Includes a cancellation token to cancel the request.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="formatter">The formatter used to serialize the value.</param>
        /// <param name="mediaType">
        /// The authoritative value of the Content-Type header. Can be null, in which case the  default
        /// content type of the formatter will be used.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of
        /// cancellation.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
        {
            var mediaTypeHeader = mediaType != null ? new MediaTypeHeaderValue(mediaType) : null;
            return client.PatchAsync(requestUri, value, formatter, mediaTypeHeader, cancellationToken);
        }

        /// <summary>
        /// Sends a PATCH request as an asynchronous operation, with a specified value serialized using the given formatter and
        /// media type.
        /// </summary>
        /// <returns>
        /// A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="client">The client used to make the request.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="value">The value to write into the entity body of the request.</param>
        /// <param name="formatter">The formatter used to serialize the value.</param>
        /// <param name="mediaType">
        /// The authoritative value of the Content-Type header. Can be null, in which case the  default
        /// content type of the formatter will be used.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of
        /// cancellation.
        /// </param>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            var method = new HttpMethod("PATCH");
            var content = new ObjectContent<T>(value, formatter, mediaType);
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            return client.SendAsync(request, cancellationToken);
        }

        #endregion

        public static async Task ValidateSuccessAsync(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var responseMessage = new StringBuilder();
                responseMessage.Append("ResponseContent: ").Append(await httpResponseMessage.Content.ReadAsStringAsync());
                responseMessage.AppendLine();
                responseMessage.Append("Response: ").Append(httpResponseMessage);
                responseMessage.AppendLine();
                responseMessage.Append("Request: ").Append(httpResponseMessage.RequestMessage);

                throw new Exception(responseMessage.ToString());
            }
        }

    }
}